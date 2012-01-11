//
// ScriptManager.cs
//
// Authors:
//   Igor Zelmanovich <igorz@mainsoft.com>
//   Marek Habersack <grendel@twistedcode.net>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
// (C) 2007-2010 Novell, Inc (http://novell.com/)
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
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Security.Permissions;
using System.Collections.Specialized;
using System.Collections;
using System.Web.Handlers;
using System.Reflection;
using System.Web.Configuration;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Xml;
using System.Collections.ObjectModel;
using System.Web.Util;

namespace System.Web.UI
{
	[ParseChildrenAttribute (true)]
	[DefaultPropertyAttribute ("Scripts")]
	[DesignerAttribute ("System.Web.UI.Design.ScriptManagerDesigner, System.Web.Extensions.Design, Version=1.0.61025.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
	[NonVisualControlAttribute]
	[PersistChildrenAttribute (false)]
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class ScriptManager : Control, IPostBackDataHandler, IScriptManager
	{
		// the keywords are used in fomatting async response
		const string updatePanel = "updatePanel";
		const string hiddenField = "hiddenField";
		const string arrayDeclaration = "arrayDeclaration";
		const string scriptBlock = "scriptBlock";
#if NET_3_5
		const string scriptStartupBlock = "scriptStartupBlock";
#endif
		const string expando = "expando";
		const string onSubmit = "onSubmit";
		const string asyncPostBackControlIDs = "asyncPostBackControlIDs";
		const string postBackControlIDs = "postBackControlIDs";
		const string updatePanelIDs = "updatePanelIDs";
		const string asyncPostBackTimeout = "asyncPostBackTimeout";
		const string childUpdatePanelIDs = "childUpdatePanelIDs";
		const string panelsToRefreshIDs = "panelsToRefreshIDs";
		const string formAction = "formAction";
		const string dataItem = "dataItem";
		const string dataItemJson = "dataItemJson";
		const string scriptDispose = "scriptDispose";
		const string pageRedirect = "pageRedirect";
		const string error = "error";
		const string pageTitle = "pageTitle";
		const string focus = "focus";
		const string scriptContentNoTags = "ScriptContentNoTags";
		const string scriptContentWithTags = "ScriptContentWithTags";
		const string scriptPath = "ScriptPath";

		static readonly object ScriptManagerKey = typeof (IScriptManager);

		int _asyncPostBackTimeout = 90;
		List<Control> _asyncPostBackControls;
		List<Control> _postBackControls;
		List<UpdatePanel> _childUpdatePanels;
		List<UpdatePanel> _panelsToRefresh;
		List<UpdatePanel> _updatePanels;
		ScriptReferenceCollection _scripts;
#if NET_3_5
		CompositeScriptReference _compositeScript;
#endif
		ServiceReferenceCollection _services;
		bool _isInAsyncPostBack;
		bool _isInPartialRendering;
		string _asyncPostBackSourceElementID;
		ScriptMode _scriptMode = ScriptMode.Auto;
		bool _enableScriptGlobalization;
		bool _enableScriptLocalization;
		string _scriptPath;
		List<RegisteredScript> _clientScriptBlocks;
		List<RegisteredScript> _startupScriptBlocks;
		List<RegisteredScript> _onSubmitStatements;
		List<RegisteredArrayDeclaration> _arrayDeclarations;
		List<RegisteredExpandoAttribute> _expandoAttributes;
		List<RegisteredHiddenField> _hiddenFields;
		List<IScriptControl> _registeredScriptControls;
		Dictionary<IExtenderControl, Control> _registeredExtenderControls;
		bool? _supportsPartialRendering;
		bool _enablePartialRendering = true;
		bool _init;
		string _panelToRefreshID;
		Dictionary<Control, DataItemEntry> _dataItems;
		bool _enablePageMethods;
		string _controlIDToFocus;
		bool _allowCustomErrorsRedirect = true;
		string _asyncPostBackErrorMessage;
		List<RegisteredDisposeScript> _disposeScripts;
		List<ScriptReferenceEntry> _scriptToRegister;
		bool _loadScriptsBeforeUI = true;
		AuthenticationServiceManager _authenticationService;
		ProfileServiceManager _profileService;
		List<ScriptManagerProxy> _proxies;

		[DefaultValue (true)]
		[Category ("Behavior")]
		public bool AllowCustomErrorsRedirect {
			get {
				return _allowCustomErrorsRedirect;
			}
			set {
				_allowCustomErrorsRedirect = value;
			}
		}

		[Category ("Behavior")]
		[DefaultValue ("")]
		public string AsyncPostBackErrorMessage {
			get {
				if (String.IsNullOrEmpty (_asyncPostBackErrorMessage))
					return String.Empty;
				return _asyncPostBackErrorMessage;
			}
			set {
				_asyncPostBackErrorMessage = value;
			}
		}

		[Browsable (false)]
		public string AsyncPostBackSourceElementID {
			get {
				if (_asyncPostBackSourceElementID == null)
					return String.Empty;
				return _asyncPostBackSourceElementID;
			}
		}

		[DefaultValue (90)]
		[Category ("Behavior")]
		public int AsyncPostBackTimeout {
			get {
				return _asyncPostBackTimeout;
			}
			set {
				_asyncPostBackTimeout = value;
			}
		}

		[Category ("Behavior")]
		[MergableProperty (false)]
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public AuthenticationServiceManager AuthenticationService {
			get {
				if (_authenticationService == null)
					_authenticationService = new AuthenticationServiceManager ();
				return _authenticationService;
			}
		}

		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool EnablePageMethods {
			get {
				return _enablePageMethods;
			}
			set {
				_enablePageMethods = value;
			}
		}

		[DefaultValue (true)]
		[Category ("Behavior")]
		public bool EnablePartialRendering {
			get {
				return _enablePartialRendering;
			}
			set {
				if (_init)
					throw new InvalidOperationException ();
				_enablePartialRendering = value;
			}
		}

		[DefaultValue (false)]
		[Category ("Behavior")]
		public bool EnableScriptGlobalization {
			get {
				return _enableScriptGlobalization;
			}
			set {
				_enableScriptGlobalization = value;
			}
		}

		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool EnableScriptLocalization {
			get {
				return _enableScriptLocalization;
			}
			set {
				_enableScriptLocalization = value;
			}
		}

		[Browsable (false)]
		public bool IsDebuggingEnabled {
			get {
				if (IsDeploymentRetail)
					return false;

				if (!RuntimeHelpers.DebuggingEnabled && (ScriptMode == ScriptMode.Auto || ScriptMode == ScriptMode.Inherit))
					return false;

				if (ScriptMode == ScriptMode.Release)
					return false;

				return true;
			}
		}

		internal bool IsDeploymentRetail {
			get {
#if TARGET_J2EE
				return false;
#else
				DeploymentSection deployment = (DeploymentSection) WebConfigurationManager.GetSection ("system.web/deployment");
				return deployment.Retail;
#endif
			}
		}

		[Browsable (false)]
		public bool IsInAsyncPostBack {
			get {
				return _isInAsyncPostBack;
			}
		}

		internal bool IsInPartialRendering {
			get {
				return _isInPartialRendering;
			}
			set {
				_isInPartialRendering = value;
			}
		}

		[Category ("Behavior")]
		[DefaultValue (true)]
		public bool LoadScriptsBeforeUI {
			get {
				return _loadScriptsBeforeUI;
			}
			set {
				_loadScriptsBeforeUI = value;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Category ("Behavior")]
		[MergableProperty (false)]
		public ProfileServiceManager ProfileService {
			get {
				if (_profileService == null)
					_profileService = new ProfileServiceManager ();
				return _profileService;
			}
		}

		[Category ("Behavior")]
#if TARGET_J2EE
		[MonoLimitation ("The 'Auto' value is the same as 'Debug'.")]
#endif
		public ScriptMode ScriptMode {
			get {
				return _scriptMode;
			}
			set {
				if (value == ScriptMode.Inherit)
					value = ScriptMode.Auto;
				_scriptMode = value;
			}
		}

		[DefaultValue ("")]
		[Category ("Behavior")]
		public string ScriptPath {
			get {
				if (_scriptPath == null)
					return String.Empty;
				return _scriptPath;
			}
			set {
				_scriptPath = value;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[Category ("Behavior")]
		[MergableProperty (false)]
		public ScriptReferenceCollection Scripts {
			get {
				if (_scripts == null)
					_scripts = new ScriptReferenceCollection ();

				return _scripts;
			}
		}
#if NET_3_5
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Category ("Behavior")]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[MergableProperty (false)]
		public CompositeScriptReference CompositeScript {
			get {
				if (_compositeScript == null)
					_compositeScript = new CompositeScriptReference ();
				return _compositeScript;
			}
		}
#endif
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[MergableProperty (false)]
		[Category ("Behavior")]
		public ServiceReferenceCollection Services {
			get {
				if (_services == null)
					_services = new ServiceReferenceCollection ();

				return _services;
			}
		}

		[DefaultValue (true)]
		[Browsable (false)]
		public bool SupportsPartialRendering {
			get {
				if (!_supportsPartialRendering.HasValue)
					_supportsPartialRendering = CheckSupportsPartialRendering ();
				return _supportsPartialRendering.Value;
			}
			set {
				if (_init)
					throw new InvalidOperationException ();
				if (!EnablePartialRendering && value)
					throw new InvalidOperationException ("The SupportsPartialRendering property cannot be set when EnablePartialRendering is false.");

				_supportsPartialRendering = value;
			}
		}

		bool CheckSupportsPartialRendering () {
			if (!EnablePartialRendering)
				return false;
			// TODO: consider browser capabilities
			return true;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool Visible {
			get {
				return true;
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Category ("Action")]
		public event EventHandler<AsyncPostBackErrorEventArgs> AsyncPostBackError;

		[Category ("Action")]
		public event EventHandler<ScriptReferenceEventArgs> ResolveScriptReference;
#if NET_3_5
		[Category ("Action")]
		public event EventHandler<CompositeScriptReferenceEventArgs> ResolveCompositeScriptReference;
#endif
		public static ScriptManager GetCurrent (Page page) {
			if (page == null)
				throw new ArgumentNullException ("page");
			return GetCurrentInternal (page);
		}

		internal static ScriptManager GetCurrentInternal (Page page)
		{
			if (page == null)
				return null;

			return (ScriptManager) page.Items [ScriptManagerKey];
		}
		
		static void SetCurrent (Page page, ScriptManager instance) {
			page.Items [ScriptManagerKey] = instance;
			page.ClientScript.RegisterWebFormClientScript ();
		}

		UpdatePanel FindPanelWithId (string id)
		{
			if (_updatePanels == null)
				return null;
			foreach (UpdatePanel panel in _updatePanels) {
				if (panel.ID == id)
					return panel;
			}
			return null;
		}
		
		protected virtual bool LoadPostData (string postDataKey, NameValueCollection postCollection) {
			_isInAsyncPostBack = true;
			string arg = postCollection [postDataKey];
			if (!String.IsNullOrEmpty (arg)) {
				string [] args = arg.Split ('|');
				switch (args.Length) {
					case 1:
						_asyncPostBackSourceElementID = args [0];
						break;

					case 2:
						_panelToRefreshID = args [0];
						_asyncPostBackSourceElementID = args [1];
						break;

					default: // "impossible situation"
						throw new InvalidOperationException ("Unexpected format of post data.");
				}
				
				return true;
			}
			return false;
		}

		protected internal virtual void OnAsyncPostBackError (AsyncPostBackErrorEventArgs e) {
			if (AsyncPostBackError != null)
				AsyncPostBackError (this, e);
		}

		protected internal override void OnInit (EventArgs e) {
			base.OnInit (e);

			if (GetCurrentInternal (Page) != null)
				throw new InvalidOperationException ("Only one instance of a ScriptManager can be added to the page.");

			SetCurrent (Page, this);
			Page.Error += new EventHandler (OnPageError);
			_init = true;
		}

		void OnPageError (object sender, EventArgs e) {
			if (IsInAsyncPostBack)
				OnAsyncPostBackError (new AsyncPostBackErrorEventArgs (Context.Error));
		}

		protected internal override void OnPreRender (EventArgs e) {
			base.OnPreRender (e);

			Page.PreRenderComplete += new EventHandler (OnPreRenderComplete);

			if (IsInAsyncPostBack) {
				Page.SetRenderMethodDelegate (RenderPageCallback);
			}
			else {
				if (EnableScriptGlobalization) {
					CultureInfo culture = Thread.CurrentThread.CurrentCulture;
					string script = String.Format ("var __cultureInfo = '{0}';", JavaScriptSerializer.DefaultSerializer.Serialize (new CultureInfoSerializer (culture)));
					RegisterClientScriptBlock (this, typeof (ScriptManager), "ScriptGlobalization", script, true);
				}

				// Register dispose script
				if (_disposeScripts != null && _disposeScripts.Count > 0) {
					StringBuilder sb = new StringBuilder ();
					sb.AppendLine ();
					for (int i = 0; i < _disposeScripts.Count; i++) {
						RegisteredDisposeScript entry = _disposeScripts [i];
						if (IsMultiForm)
							sb.Append ("Sys.WebForms.PageRequestManager.getInstance($get(\"" + Page.Form.ClientID + "\"))._registerDisposeScript(\"");
						else
							sb.Append ("Sys.WebForms.PageRequestManager.getInstance()._registerDisposeScript(\"");
						sb.Append (entry.UpdatePanel.ClientID);
						sb.Append ("\", ");
						sb.Append (JavaScriptSerializer.DefaultSerializer.Serialize (entry.Script)); //JavaScriptSerializer.Serialize used escape script literal 
						sb.AppendLine (");");
					}
					RegisterStartupScript (this, typeof (ExtenderControl), "disposeScripts;", sb.ToString (), true);
				}

#if TARGET_DOTNET
				// to cause webform client script being included
				Page.ClientScript.GetPostBackEventReference (new PostBackOptions (this, null, null, false, false, false, true, true, null));
#else
				Page.ClientScript.GetPostBackEventReference (this, null);
#endif
			}
		}

		ScriptReference CreateScriptReference (string path, string assembly, bool notifyScriptLoaded)
		{
			var ret = new ScriptReference (path, assembly);
			if (!notifyScriptLoaded)
				ret.NotifyScriptLoaded = false;
			OnResolveScriptReference (new ScriptReferenceEventArgs (ret));

			return ret;
		}

		ScriptReference CreateScriptReference (string path, string assembly)
		{
			return CreateScriptReference (path, assembly, true);
		}
		
		void OnPreRenderComplete (object sender, EventArgs e)
		{
			// Resolve Scripts
			ScriptReference ajaxScript = CreateScriptReference ("MicrosoftAjax.js", String.Empty, false);
			ScriptReference ajaxWebFormsScript = CreateScriptReference ("MicrosoftAjaxWebForms.js", String.Empty, false);
			ScriptReference ajaxExtensionScript = null;
			ScriptReference ajaxWebFormsExtensionScript = null;
			if (IsMultiForm) {
				ajaxExtensionScript = CreateScriptReference ("MicrosoftAjaxExtension.js", String.Empty);
				ajaxWebFormsExtensionScript = CreateScriptReference ("MicrosoftAjaxWebFormsExtension.js", String.Empty);
			}

			foreach (ScriptReferenceEntry script in GetScriptReferences ()) {
				OnResolveScriptReference (new ScriptReferenceEventArgs (script.ScriptReference));
					if (_scriptToRegister == null)
						_scriptToRegister = new List<ScriptReferenceEntry> ();
					_scriptToRegister.Add (script);
			}

			if (!IsInAsyncPostBack) {
				// Register Ajax framework script.
				RegisterScriptReference (this, ajaxScript, true);
				if (IsMultiForm) {
					RegisterScriptReference (this, ajaxExtensionScript, true);
					RegisterClientScriptBlock (this, typeof (ScriptManager), "Sys.Application", "\nSys.Application._initialize(document.getElementById('" + Page.Form.ClientID + "'));\n", true);
				}

				StringBuilder sb = new StringBuilder ();
   				sb.AppendLine ("if (typeof(Sys) === 'undefined') throw new Error('ASP.NET Ajax client-side framework failed to load.');");

				ScriptingProfileServiceSection profileService = (ScriptingProfileServiceSection) WebConfigurationManager.GetSection ("system.web.extensions/scripting/webServices/profileService");
				if (profileService != null && profileService.Enabled)
					sb.AppendLine ("Sys.Services._ProfileService.DefaultWebServicePath = '" + ResolveClientUrl ("~" + System.Web.Script.Services.ProfileService.DefaultWebServicePath) + "';");
				string profileServicePath = GetProfileServicePath ();
				if (!String.IsNullOrEmpty (profileServicePath))
					sb.AppendLine ("Sys.Services.ProfileService.set_path('" + profileServicePath + "');");

				ScriptingAuthenticationServiceSection authenticationService = (ScriptingAuthenticationServiceSection) WebConfigurationManager.GetSection ("system.web.extensions/scripting/webServices/authenticationService");
				if (authenticationService != null && authenticationService.Enabled) {
					sb.AppendLine ("Sys.Services._AuthenticationService.DefaultWebServicePath = '" + ResolveClientUrl ("~/Authentication_JSON_AppService.axd") + "';");
					if (Page.User.Identity.IsAuthenticated)
						sb.AppendLine ("Sys.Services.AuthenticationService._setAuthenticated(true);");
				}
				string authenticationServicePath = GetAuthenticationServicePath ();
				if (!String.IsNullOrEmpty (authenticationServicePath))
					sb.AppendLine ("Sys.Services.AuthenticationService.set_path('" + authenticationServicePath + "');");

				RegisterClientScriptBlock (this, typeof (ScriptManager), "Framework", sb.ToString (), true);

				RegisterScriptReference (this, ajaxWebFormsScript, true);

				if (IsMultiForm)
					RegisterScriptReference (this, ajaxWebFormsExtensionScript, true);
			}
#if NET_3_5
			if (_compositeScript != null && _compositeScript.HaveScripts ()) {
				OnResolveCompositeScriptReference (new CompositeScriptReferenceEventArgs (_compositeScript));
				RegisterScriptReference (this, _compositeScript, true);
			}
#endif			
			// Register Scripts
			if (_scriptToRegister != null)
				for (int i = 0; i < _scriptToRegister.Count; i++)
					RegisterScriptReference (_scriptToRegister [i].Control, _scriptToRegister [i].ScriptReference, _scriptToRegister [i].LoadScriptsBeforeUI);

			if (!IsInAsyncPostBack) {
				// Register services
				if (_services != null && _services.Count > 0) {
					for (int i = 0; i < _services.Count; i++) {
						RegisterServiceReference (this, _services [i]);
					}
				}

				if (_proxies != null && _proxies.Count > 0) {
					for (int i = 0; i < _proxies.Count; i++) {
						ScriptManagerProxy proxy = _proxies [i];
						for (int j = 0; j < proxy.Services.Count; j++) {
							RegisterServiceReference (proxy, proxy.Services [j]);
						}
					}
				}

				if (EnablePageMethods) {
					LogicalTypeInfo logicalTypeInfo = LogicalTypeInfo.GetLogicalTypeInfo (Page.GetType (), Page.Request.FilePath);
					RegisterClientScriptBlock (this, typeof (ScriptManager), "PageMethods", logicalTypeInfo.Proxy, true);
				}

				// Register startup script
				if (IsMultiForm)
					RegisterStartupScript (this, typeof (ExtenderControl), "Sys.Application.initialize();", "Sys.Application.getInstance($get(\"" + Page.Form.ClientID + "\")).initialize();\n", true);
				else
					RegisterStartupScript (this, typeof (ExtenderControl), "Sys.Application.initialize();", "Sys.Application.initialize();\n", true);
			}
		}

		string GetProfileServicePath () {
			if (_profileService != null && !String.IsNullOrEmpty (_profileService.Path))
				return ResolveClientUrl (_profileService.Path);

			if (_proxies != null && _proxies.Count > 0)
				for (int i = 0; i < _proxies.Count; i++)
					if (!String.IsNullOrEmpty (_proxies [i].ProfileService.Path))
						return _proxies [i].ResolveClientUrl (_proxies [i].ProfileService.Path);
			return null;
		}

		string GetAuthenticationServicePath () {
			if (_authenticationService != null && !String.IsNullOrEmpty (_authenticationService.Path))
				return ResolveClientUrl (_authenticationService.Path);

			if (_proxies != null && _proxies.Count > 0)
				for (int i = 0; i < _proxies.Count; i++)
					if (!String.IsNullOrEmpty (_proxies [i].AuthenticationService.Path))
						return _proxies [i].ResolveClientUrl (_proxies [i].AuthenticationService.Path);
			return null;
		}

		public ReadOnlyCollection<RegisteredArrayDeclaration> GetRegisteredArrayDeclarations () {
			if (_arrayDeclarations == null)
				_arrayDeclarations = new List<RegisteredArrayDeclaration> ();
			return new ReadOnlyCollection<RegisteredArrayDeclaration> (_arrayDeclarations);
		}

		public ReadOnlyCollection<RegisteredScript> GetRegisteredClientScriptBlocks () {
			if (_clientScriptBlocks == null)
				_clientScriptBlocks = new List<RegisteredScript> ();
			return new ReadOnlyCollection<RegisteredScript> (_clientScriptBlocks);
		}

		public ReadOnlyCollection<RegisteredDisposeScript> GetRegisteredDisposeScripts () {
			if (_disposeScripts == null)
				_disposeScripts = new List<RegisteredDisposeScript> ();
			return new ReadOnlyCollection<RegisteredDisposeScript> (_disposeScripts);
		}

		public ReadOnlyCollection<RegisteredExpandoAttribute> GetRegisteredExpandoAttributes () {
			if (_expandoAttributes == null)
				_expandoAttributes = new List<RegisteredExpandoAttribute> ();
			return new ReadOnlyCollection<RegisteredExpandoAttribute> (_expandoAttributes);
		}

		public ReadOnlyCollection<RegisteredHiddenField> GetRegisteredHiddenFields () {
			if (_hiddenFields == null)
				_hiddenFields = new List<RegisteredHiddenField> ();
			return new ReadOnlyCollection<RegisteredHiddenField> (_hiddenFields);
		}

		public ReadOnlyCollection<RegisteredScript> GetRegisteredOnSubmitStatements () {
			if (_onSubmitStatements == null)
				_onSubmitStatements = new List<RegisteredScript> ();
			return new ReadOnlyCollection<RegisteredScript> (_onSubmitStatements);
		}

		public ReadOnlyCollection<RegisteredScript> GetRegisteredStartupScripts () {
			if (_startupScriptBlocks == null)
				_startupScriptBlocks = new List<RegisteredScript> ();
			return new ReadOnlyCollection<RegisteredScript> (_startupScriptBlocks);
		}

#if TARGET_J2EE
		bool _isMultiForm = false;
		bool _isMultiFormInited = false;

		bool IsMultiForm {
			get {
				if (!_isMultiFormInited) {
					string isMultiForm = WebConfigurationManager.AppSettings ["mainsoft.use.portlet.namespace"];
					_isMultiForm = isMultiForm != null ? Boolean.Parse (isMultiForm) : false;

					_isMultiFormInited = true;
				}
				return _isMultiForm;
			}
		}
#else
		bool IsMultiForm {
			get { return false; }
		}
#endif

		bool PanelRequiresUpdate (UpdatePanel panel)
		{
			if (panel == null || _panelsToRefresh == null || _panelsToRefresh.Count == 0)
				return false;

			return _panelsToRefresh.Contains (panel);
		}
		
		bool HasBeenRendered (Control control)
		{
			if (control == null)
				return false;

			UpdatePanel panel = control as UpdatePanel;
			if (PanelRequiresUpdate (panel))
				return true;

			return HasBeenRendered (control.Parent);
		}

		IEnumerable<ScriptReferenceEntry> GetScriptReferences () {
			if (_scripts != null && _scripts.Count > 0) {
				for (int i = 0; i < _scripts.Count; i++) {
					yield return new ScriptReferenceEntry (this, _scripts [i], LoadScriptsBeforeUI);
				}
			}

			if (_proxies != null && _proxies.Count > 0) {
				for (int i = 0; i < _proxies.Count; i++) {
					ScriptManagerProxy proxy = _proxies [i];
					for (int j = 0; j < proxy.Scripts.Count; j++)
						yield return new ScriptReferenceEntry (proxy, proxy.Scripts [j], LoadScriptsBeforeUI);
				}
			}

			if (_registeredScriptControls != null && _registeredScriptControls.Count > 0) {
				for (int i = 0; i < _registeredScriptControls.Count; i++) {
					IEnumerable<ScriptReference> scripts = _registeredScriptControls [i].GetScriptReferences ();
					if (scripts != null)
						foreach (ScriptReference s in scripts)
							yield return new ScriptReferenceEntry ((Control) _registeredScriptControls [i], s, LoadScriptsBeforeUI);
				}
			}

			if (_registeredExtenderControls != null && _registeredExtenderControls.Count > 0) {
				foreach (IExtenderControl ex in _registeredExtenderControls.Keys) {
					IEnumerable<ScriptReference> scripts = ex.GetScriptReferences ();
					if (scripts != null)
						foreach (ScriptReference s in scripts)
							yield return new ScriptReferenceEntry ((Control) ex, s, LoadScriptsBeforeUI);
				}
			}
		}
#if NET_3_5
		protected virtual void OnResolveCompositeScriptReference (CompositeScriptReferenceEventArgs e)
		{
			EventHandler <CompositeScriptReferenceEventArgs> evt = ResolveCompositeScriptReference;
			if (evt != null)
				evt (this, e);
		}
#endif
		protected virtual void OnResolveScriptReference (ScriptReferenceEventArgs e) {
			if (ResolveScriptReference != null)
				ResolveScriptReference (this, e);
		}

		protected virtual void RaisePostDataChangedEvent ()
		{
			// Why override?
		}

		public static void RegisterArrayDeclaration (Page page, string arrayName, string arrayValue) {
			RegisterArrayDeclaration ((Control) page, arrayName, arrayValue);
		}

		public static void RegisterArrayDeclaration (Control control, string arrayName, string arrayValue) {
			Page page = control.Page;
			ScriptManager sm = GetCurrentInternal (page);

			if (sm == null)
				return;
			
			if (sm._arrayDeclarations == null)
				sm._arrayDeclarations = new List<RegisteredArrayDeclaration> ();

			sm._arrayDeclarations.Add (new RegisteredArrayDeclaration (control, arrayName, arrayValue));

			if (!sm.IsInAsyncPostBack)
				page.ClientScript.RegisterArrayDeclaration (arrayName, arrayValue);
		}

		public void RegisterAsyncPostBackControl (Control control) {
			if (control == null)
				return;

			if (_asyncPostBackControls == null)
				_asyncPostBackControls = new List<Control> ();

			if (_asyncPostBackControls.Contains (control))
				return;

			_asyncPostBackControls.Add (control);
		}

		public static void RegisterClientScriptBlock (Page page, Type type, string key, string script, bool addScriptTags) {
			RegisterClientScriptBlock ((Control) page, type, key, script, addScriptTags);
		}

		public static void RegisterClientScriptBlock (Control control, Type type, string key, string script, bool addScriptTags) {
			Page page = control.Page;
			ScriptManager sm = GetCurrentInternal (page);

			if (sm == null)
				return;
			
			RegisterScript (ref sm._clientScriptBlocks, control, type, key, script, null, addScriptTags, RegisteredScriptType.ClientScriptBlock);

			if (!sm.IsInAsyncPostBack)
				page.ClientScript.RegisterClientScriptBlock (type, key, script, addScriptTags);
		}

		public static void RegisterClientScriptInclude (Page page, Type type, string key, string url) {
			RegisterClientScriptInclude ((Control) page, type, key, url);
		}

		public static void RegisterClientScriptInclude (Control control, Type type, string key, string url) {
			Page page = control.Page;
			ScriptManager sm = GetCurrentInternal (page);

			if (sm == null)
				return;
			
			RegisterScript (ref sm._clientScriptBlocks, control, type, key, null, url, false, RegisteredScriptType.ClientScriptInclude);

			if (!sm.IsInAsyncPostBack)
				page.ClientScript.RegisterClientScriptInclude (type, key, url);
		}

		public static void RegisterClientScriptResource (Page page, Type type, string resourceName) {
			RegisterClientScriptResource ((Control) page, type, resourceName);
		}

		public static void RegisterClientScriptResource (Control control, Type type, string resourceName) {
			RegisterClientScriptInclude (control, type, resourceName, ScriptResourceHandler.GetResourceUrl (type.Assembly, resourceName, true));
		}

		void RegisterScriptReference (Control control, ScriptReferenceBase script, bool loadScriptsBeforeUI)
		{
			string scriptPath = script.Path;
			string url = String.IsNullOrEmpty (scriptPath) ? script.GetUrl (this, false) : scriptPath;
			if (control != this && !String.IsNullOrEmpty (scriptPath))
				url = control.ResolveClientUrl (scriptPath);

			if (String.IsNullOrEmpty (url))
				return;
			
			if (loadScriptsBeforeUI)
				RegisterClientScriptInclude (control, typeof (ScriptManager), url, url);
			else
				RegisterStartupScript (control, typeof (ScriptManager), url, String.Format ("<script src=\"{0}\" type=\"text/javascript\"></script>", url), false);
		}

		void RegisterServiceReference (Control control, ServiceReference serviceReference)
		{
			if (serviceReference.InlineScript) {
				string url = control.ResolveUrl (serviceReference.Path);
				Type type = WebServiceParser.GetCompiledType (url, Context);
				if (type != null) {
					object[] attributes = type.GetCustomAttributes (typeof (ScriptServiceAttribute), true);
					if (attributes.Length == 0)
						throw new InvalidOperationException ("Only Web services with a [ScriptService] attribute on the class definition can be called from script.");
				}
				
				LogicalTypeInfo logicalTypeInfo = LogicalTypeInfo.GetLogicalTypeInfo (type, url);
				RegisterClientScriptBlock (control, typeof (ScriptManager), url, logicalTypeInfo.Proxy, true);
			}
			else {
#if TARGET_J2EE
				string pathInfo = "/js.invoke";
#else
				string pathInfo = "/js";
				if (IsDebuggingEnabled)
					pathInfo += "debug";
#endif
				string url = String.Concat (control.ResolveClientUrl (serviceReference.Path), pathInfo);
				RegisterClientScriptInclude (control, typeof (ScriptManager), url, url);
			}
		}

		public void RegisterDataItem (Control control, string dataItem) {
			RegisterDataItem (control, dataItem, false);
		}

		public void RegisterDataItem (Control control, string dataItem, bool isJsonSerialized) {
			if (!IsInAsyncPostBack)
				throw new InvalidOperationException ("RegisterDataItem can only be called during an async postback.");
			if (control == null)
				throw new ArgumentNullException ("control");

			if (_dataItems == null)
				_dataItems = new Dictionary<Control, DataItemEntry> ();

			if (_dataItems.ContainsKey (control))
				throw new ArgumentException (String.Format ("'{0}' already has a data item registered.", control.ID), "control");

			_dataItems.Add (control, new DataItemEntry (dataItem, isJsonSerialized));
		}

		public void RegisterDispose (Control control, string disposeScript) {
			if (control == null)
				throw new ArgumentNullException ("control");
			if (disposeScript == null)
				throw new ArgumentNullException ("disposeScript");

			UpdatePanel updatePanel = GetUpdatePanel (control);
			if (updatePanel == null)
				return;

			if (_disposeScripts == null)
				_disposeScripts = new List<RegisteredDisposeScript> ();
			_disposeScripts.Add (new RegisteredDisposeScript (control, disposeScript, updatePanel));
		}

		static UpdatePanel GetUpdatePanel (Control control) {
			if (control == null)
				return null;

			UpdatePanel parent = control.Parent as UpdatePanel;
			if (parent != null)
				return parent;

			return GetUpdatePanel (control.Parent);
		}

		public static void RegisterExpandoAttribute (Control control, string controlId, string attributeName, string attributeValue, bool encode) {
			Page page = control.Page;
			ScriptManager sm = GetCurrentInternal (page);

			if (sm == null)
				return;
			
			if (sm._expandoAttributes == null)
				sm._expandoAttributes = new List<RegisteredExpandoAttribute> ();

			sm._expandoAttributes.Add (new RegisteredExpandoAttribute (control, controlId, attributeName, attributeValue, encode));

			if (!sm.IsInAsyncPostBack)
				page.ClientScript.RegisterExpandoAttribute (controlId, attributeName, attributeValue, encode);
		}

		public static void RegisterHiddenField (Page page, string hiddenFieldName, string hiddenFieldInitialValue) {
			RegisterHiddenField ((Control) page, hiddenFieldName, hiddenFieldInitialValue);
		}

		public static void RegisterHiddenField (Control control, string hiddenFieldName, string hiddenFieldInitialValue) {
			Page page = control.Page;
			ScriptManager sm = GetCurrentInternal (page);

			if (sm == null)
				return;
			
			if (sm._hiddenFields == null)
				sm._hiddenFields = new List<RegisteredHiddenField> ();

			sm._hiddenFields.Add (new RegisteredHiddenField (control, hiddenFieldName, hiddenFieldInitialValue));

			if (!sm.IsInAsyncPostBack)
				page.ClientScript.RegisterHiddenField (hiddenFieldName, hiddenFieldInitialValue);
		}

		public static void RegisterOnSubmitStatement (Page page, Type type, string key, string script) {
			RegisterOnSubmitStatement ((Control) page, type, key, script);
		}

		public static void RegisterOnSubmitStatement (Control control, Type type, string key, string script) {
			Page page = control.Page;
			ScriptManager sm = GetCurrentInternal (page);

			if (sm == null)
				return;
			
			RegisterScript (ref sm._onSubmitStatements, control, type, key, script, null, false, RegisteredScriptType.OnSubmitStatement);

			if (!sm.IsInAsyncPostBack)
				page.ClientScript.RegisterOnSubmitStatement (type, key, script);
		}

		public void RegisterPostBackControl (Control control) {
			if (control == null)
				return;

			if (_postBackControls == null)
				_postBackControls = new List<Control> ();

			if (_postBackControls.Contains (control))
				return;

			_postBackControls.Add (control);
		}

		internal void RegisterUpdatePanel (UpdatePanel updatePanel) {
			if (_updatePanels == null)
				_updatePanels = new List<UpdatePanel> ();

			if (_updatePanels.Contains (updatePanel))
				return;

			_updatePanels.Add (updatePanel);
		}

		public void RegisterScriptDescriptors (IExtenderControl extenderControl) {
			if (extenderControl == null)
				return;

			if (_registeredExtenderControls == null || !_registeredExtenderControls.ContainsKey (extenderControl))
				return;

			Control targetControl = _registeredExtenderControls [extenderControl];
			RegisterScriptDescriptors ((Control) extenderControl, extenderControl.GetScriptDescriptors (targetControl));
		}

		public void RegisterScriptDescriptors (IScriptControl scriptControl) {
			if (scriptControl == null)
				return;

			if (_registeredScriptControls == null || !_registeredScriptControls.Contains (scriptControl))
				return;

			RegisterScriptDescriptors ((Control) scriptControl, scriptControl.GetScriptDescriptors ());
		}

		void RegisterScriptDescriptors (Control control, IEnumerable<ScriptDescriptor> scriptDescriptors)
		{
			if (scriptDescriptors == null)
				return;

			StringBuilder sb = new StringBuilder ();
			foreach (ScriptDescriptor scriptDescriptor in scriptDescriptors) {
				if (IsMultiForm) {
					scriptDescriptor.FormID = Page.Form.ClientID;
					sb.AppendLine ("Sys.Application.getInstance($get(\"" + Page.Form.ClientID + "\")).add_init(function() {");
				}
				else
					sb.AppendLine ("Sys.Application.add_init(function() {");
				sb.Append ("\t");
				sb.AppendLine (scriptDescriptor.GetScript ());
				sb.AppendLine ("});");
			}
			string script = sb.ToString ();
			RegisterStartupScript (control, typeof (ScriptDescriptor), script, script, true);
		}

		public static void RegisterStartupScript (Page page, Type type, string key, string script, bool addScriptTags) {
			RegisterStartupScript ((Control) page, type, key, script, addScriptTags);
		}

		public static void RegisterStartupScript (Control control, Type type, string key, string script, bool addScriptTags) {
			Page page = control.Page;
			ScriptManager sm = GetCurrentInternal (page);

			if (sm == null)
				return;
			
			RegisterScript (ref sm._startupScriptBlocks, control, type, key, script, null, addScriptTags, RegisteredScriptType.ClientStartupScript);

			if (!sm.IsInAsyncPostBack)
				page.ClientScript.RegisterStartupScript (type, key, script, addScriptTags);
		}

		public void RegisterScriptControl<TScriptControl> (TScriptControl scriptControl) where TScriptControl : Control, IScriptControl {
			if (scriptControl == null)
				throw new ArgumentNullException ("scriptControl");

			if (_registeredScriptControls == null)
				_registeredScriptControls = new List<IScriptControl> ();

			if (!_registeredScriptControls.Contains (scriptControl))
				_registeredScriptControls.Add (scriptControl);
		}

		public void RegisterExtenderControl<TExtenderControl> (TExtenderControl extenderControl, Control targetControl) where TExtenderControl : Control, IExtenderControl {
			if (extenderControl == null)
				throw new ArgumentNullException ("extenderControl");
			if (targetControl == null)
				throw new ArgumentNullException ("targetControl");

			if (_registeredExtenderControls == null)
				_registeredExtenderControls = new Dictionary<IExtenderControl, Control> ();

			if (!_registeredExtenderControls.ContainsKey (extenderControl))
				_registeredExtenderControls.Add (extenderControl, targetControl);
		}

		static void RegisterScript (ref List<RegisteredScript> scriptList, Control control, Type type, string key, string script, string url, bool addScriptTag, RegisteredScriptType scriptType) {
			if (scriptList == null)
				scriptList = new List<RegisteredScript> ();

			scriptList.Add (new RegisteredScript (control, type, key, script, url, addScriptTag, scriptType));
		}

		protected internal override void Render (HtmlTextWriter writer) {
			// MSDN: This method is used by control developers to extend the ScriptManager control. 
			// Notes to Inheritors: 
			// When overriding this method, call the base Render(HtmlTextWriter) method 
			// so that PageRequestManager is rendered on the page.
			if (SupportsPartialRendering) {
				writer.WriteLine ("<script type=\"text/javascript\">");
				writer.WriteLine ("//<![CDATA[");
				writer.WriteLine ("Sys.WebForms.PageRequestManager._initialize('{0}', document.getElementById('{1}'));", UniqueID, Page.Form.ClientID);
				if (IsMultiForm)
					writer.WriteLine ("Sys.WebForms.PageRequestManager.getInstance($get(\"{0}\"))._updateControls([{1}], [{2}], [{3}], {4});",
							  Page.Form.ClientID,
							  FormatUpdatePanelIDs (_updatePanels, true),
							  FormatListIDs (_asyncPostBackControls, true, false),
							  FormatListIDs (_postBackControls, true, false),
							  AsyncPostBackTimeout);
				else
					writer.WriteLine ("Sys.WebForms.PageRequestManager.getInstance()._updateControls([{0}], [{1}], [{2}], {3});",
							  FormatUpdatePanelIDs (_updatePanels, true),
							  FormatListIDs (_asyncPostBackControls, true, false),
							  FormatListIDs (_postBackControls, true, false),
							  AsyncPostBackTimeout);
				writer.WriteLine ("//]]");
				writer.WriteLine ("</script>");
			}
			base.Render (writer);
		}

		static string FormatUpdatePanelIDs (List<UpdatePanel> list, bool useSingleQuote) {
			return FormatUpdatePanelIDs (list, useSingleQuote, false);
		}
		
		static string FormatUpdatePanelIDs (List<UpdatePanel> list, bool useSingleQuote, bool useUntaggedName) {
			if (list == null || list.Count == 0)
				return null;

			string quote = useSingleQuote ? "'" : String.Empty;
			string id;
			StringBuilder sb = new StringBuilder ();
			foreach (UpdatePanel panel in list) {
				if (!panel.Visible)
				 	continue;

				id = panel.UniqueID;
				sb.AppendFormat ("{0}{1}{2}{0},", quote, panel.ChildrenAsTriggers ? "t" : "f", id);
				if (useUntaggedName)
					sb.AppendFormat ("{0}{1}{0},", quote, id);
			}
			
			if (sb.Length > 0)
				sb.Length--;

			return sb.ToString ();
		}

		static string FormatListIDs<T> (List<T> list, bool useSingleQuote, bool skipInvisible) where T : Control
		{
			if (list == null || list.Count == 0)
				return null;

			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < list.Count; i++) {
				if (skipInvisible && !list [i].Visible)
					continue;
				
				sb.AppendFormat ("{0}{1}{0},", useSingleQuote ? "'" : String.Empty, list [i].UniqueID);
			}
			if (sb.Length > 0)
				sb.Length--;

			return sb.ToString ();
		}

		public void SetFocus (Control control) {
			if (control == null)
				throw new ArgumentNullException ("control");

			if (IsInAsyncPostBack) {
				EnsureFocusClientScript ();
				_controlIDToFocus = control.ClientID;
			}
			else
				Page.SetFocus (control);
		}

		public void SetFocus (string clientID) {
			if (String.IsNullOrEmpty (clientID))
				throw new ArgumentNullException ("control");

			if (IsInAsyncPostBack) {
				EnsureFocusClientScript ();
				_controlIDToFocus = clientID;
			}
			else
				Page.SetFocus (clientID);
		}

		void EnsureFocusClientScript () {
#if	TARGET_DOTNET
			RegisterClientScriptResource (this, typeof (ClientScriptManager), "Focus.js");
#endif
		}

		#region IPostBackDataHandler Members

		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection) {
			return LoadPostData (postDataKey, postCollection);
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent () {
			RaisePostDataChangedEvent ();
		}

		#endregion

		internal static void WriteCallbackException (ScriptManager current, TextWriter output, Exception ex, bool writeMessage)
		{
#if TARGET_DOTNET
			if (ex is HttpUnhandledException)
				ex = ex.InnerException;
#endif
			HttpException httpEx = ex as HttpException;
			string message = current != null ? current.AsyncPostBackErrorMessage : null;
			if (String.IsNullOrEmpty (message) && writeMessage) {
				if ((current != null && current.IsDebuggingEnabled) || (current == null && RuntimeHelpers.DebuggingEnabled))
					message = ex.ToString ();
				else
					message = ex.Message;
			}
			
			WriteCallbackOutput (output, error, httpEx == null ? "500" : httpEx.GetHttpCode ().ToString (), message);
		}

		static internal void WriteCallbackRedirect (TextWriter output, string redirectUrl) {
			WriteCallbackOutput (output, pageRedirect, null, redirectUrl);
		}

		void RegisterPanelForRefresh (UpdatePanel panel)
		{
			if (_panelsToRefresh == null)
				_panelsToRefresh = new List<UpdatePanel> ();
			else if (_panelsToRefresh.Contains (panel))
				return;

			_panelsToRefresh.Add (panel);
			RegisterUpdatePanel (panel);
		}
		
		internal void WriteCallbackPanel (TextWriter output, UpdatePanel panel, StringBuilder panelOutput)
		{
			WriteCallbackOutput (output, updatePanel, panel.ClientID, panelOutput);
		}

		internal void RegisterChildUpdatePanel (UpdatePanel updatePanel) {
			if (_childUpdatePanels == null)
				_childUpdatePanels = new List<UpdatePanel> ();
			_childUpdatePanels.Add (updatePanel);
		}

		static void WriteCallbackOutput (TextWriter output, string type, string name, object value) {
			string str = value as string;
			StringBuilder sb = str == null ? value as StringBuilder : null;
			int length = 0;
			if (str != null)
				length = str.Length;
			else if (sb != null)
				length = sb.Length;

			//output.Write ("{0}|{1}|{2}|{3}|", value == null ? 0 : value.Length, type, name, value);
			output.Write (length);
			output.Write ('|');
			output.Write (type);
			output.Write ('|');
			output.Write (name);
			output.Write ('|');
			for (int i = 0; i < length; i++)
				if (str != null)
					output.Write (str [i]);
				else
					output.Write (sb [i]);
			output.Write ('|');
		}

		void RenderPageCallback (HtmlTextWriter output, Control container)
		{
			Page page = (Page) container;

			// MSDN: http://msdn.microsoft.com/en-us/library/system.web.ui.updatepanel.aspx
			//
			// Refreshing UpdatePanel Content
			//
			// When partial-page rendering is enabled, a control can perform a postback
			// that updates the whole page or an asynchronous postback that updates the
			// content of one or more UpdatePanel controls. Whether a control causes an
			// asynchronous postback and updates an UpdatePanel control depends on the
			// following:
			//
			// * If the UpdateMode property of the UpdatePanel control is set to Always,
			//   the UpdatePanel control's content is updated on every postback that
			//   originates from the page. This includes asynchronous postbacks from
			//   controls that are inside other UpdatePanel controls and postbacks from
			//   controls that are not inside UpdatePanel controls.
			//
			// * If the UpdateMode property is set to Conditional, the UpdatePanel
			//   control's content is updated in the following circumstances:
			//
			//       o When you call the Update method of the UpdatePanel control
			//         explicitly.
			//       o When the UpdatePanel control is nested inside another UpdatePanel
			//         control, and the parent panel is updated.
			//       o When a postback is caused by a control that is defined as a
			//         trigger by using the Triggers property of the UpdatePanel
			//         control. In this scenario, the control explicitly triggers an
			//         update of the panel content. The control can be either inside or
			//         outside the UpdatePanel control that the trigger is associated
			//         with.
			//       o When the ChildrenAsTriggers property is set to true and a child
			//         control of the UpdatePanel control causes a postback. Child
			//         controls of nested UpdatePanel controls do not cause an update to
			//         the outer UpdatePanel control unless they are explicitly defined
			//         as triggers.
			//
			if (_updatePanels != null && _updatePanels.Count > 0) {
				bool needsUpdate;
				foreach (UpdatePanel panel in _updatePanels) {
					if (panel.RequiresUpdate || (!String.IsNullOrEmpty (_panelToRefreshID) && String.Compare (_panelToRefreshID, panel.UniqueID, StringComparison.Ordinal) == 0))
						needsUpdate = true;
					else
						needsUpdate = false;

					panel.SetInPartialRendering (needsUpdate);
					if (needsUpdate)
						RegisterPanelForRefresh (panel);
				}
			}
			
			page.Form.SetRenderMethodDelegate (RenderFormCallback);
			HtmlTextParser parser = new HtmlTextParser (output);
			page.Form.RenderControl (parser);

			Dictionary <string, string> pageHiddenFields = parser.HiddenFields;
			if (pageHiddenFields != null)
				foreach (KeyValuePair <string, string> kvp in pageHiddenFields)
					WriteCallbackOutput (output, hiddenField, kvp.Key, kvp.Value);
			
			WriteCallbackOutput (output, asyncPostBackControlIDs, null, FormatListIDs (_asyncPostBackControls, false, false));
			WriteCallbackOutput (output, postBackControlIDs, null, FormatListIDs (_postBackControls, false, false));
			WriteCallbackOutput (output, updatePanelIDs, null, FormatUpdatePanelIDs (_updatePanels, false, true));
			WriteCallbackOutput (output, childUpdatePanelIDs, null, FormatListIDs (_childUpdatePanels, false, true));
			WriteCallbackOutput (output, panelsToRefreshIDs, null, FormatListIDs (_panelsToRefresh, false, true));
			WriteCallbackOutput (output, asyncPostBackTimeout, null, AsyncPostBackTimeout.ToString ());
			if (!IsMultiForm)
				WriteCallbackOutput (output, pageTitle, null, Page.Title);

			if (_dataItems != null)
				foreach (Control control in _dataItems.Keys) {
					DataItemEntry entry = _dataItems [control];
					WriteCallbackOutput (output, entry.IsJsonSerialized ? dataItemJson : dataItem, control.ClientID, entry.DataItem);
				}

			WriteArrayDeclarations (output);
			WriteExpandoAttributes (output);
			WriteScriptBlocks (output, _clientScriptBlocks);
			WriteScriptBlocks (output, _startupScriptBlocks);
			WriteScriptBlocks (output, _onSubmitStatements);
			WriteHiddenFields (output);

			if (!String.IsNullOrEmpty (_controlIDToFocus))
				WriteCallbackOutput (output, focus, null, _controlIDToFocus);

			if (_disposeScripts != null)
				for (int i = 0; i < _disposeScripts.Count; i++) {
					RegisteredDisposeScript entry = _disposeScripts [i];
					if ((_panelsToRefresh != null && _panelsToRefresh.IndexOf (entry.UpdatePanel) >= 0) || (_childUpdatePanels != null && _childUpdatePanels.IndexOf (entry.UpdatePanel) >= 0))
						WriteCallbackOutput (output, scriptDispose, entry.UpdatePanel.ClientID, entry.Script);
				}
		}

		private void WriteExpandoAttributes (HtmlTextWriter writer) {
			if (_expandoAttributes != null) {
				for (int i = 0; i < _expandoAttributes.Count; i++) {
					RegisteredExpandoAttribute attr = _expandoAttributes [i];
					if (HasBeenRendered (attr.Control)) {
						string value;
						if (attr.Encode) {
							StringWriter sw = new StringWriter ();
							Newtonsoft.Json.JavaScriptUtils.WriteEscapedJavaScriptString (attr.Value, sw);
							value = sw.ToString ();
						}
						else
							value = "\"" + attr.Value + "\"";
						WriteCallbackOutput (writer, expando, "document.getElementById('" + attr.ControlId + "')['" + attr.Name + "']", value);
					}
				}
			}
		}

		void WriteArrayDeclarations (HtmlTextWriter writer) {
			if (_arrayDeclarations != null) {
				for (int i = 0; i < _arrayDeclarations.Count; i++) {
					RegisteredArrayDeclaration array = _arrayDeclarations [i];
					if (Page == array.Control || HasBeenRendered (array.Control))
						WriteCallbackOutput (writer, arrayDeclaration, array.Name, array.Value);
				}
			}
		}

		void WriteScriptBlocks (HtmlTextWriter output, List<RegisteredScript> scriptList) {
			if (scriptList == null)
				return;
			var registeredScripts = new Dictionary <string, RegisteredScript> ();
			Control control;
			Page page = Page;
			
			for (int i = 0; i < scriptList.Count; i++) {
				RegisteredScript scriptEntry = scriptList [i];
				if (registeredScripts.ContainsKey (scriptEntry.Key))
					continue;

 				control = scriptEntry.Control;
				if (page == control || HasBeenRendered (control)) {
					registeredScripts.Add (scriptEntry.Key, scriptEntry);
					switch (scriptEntry.ScriptType) {
						case RegisteredScriptType.ClientScriptBlock:
							if (scriptEntry.AddScriptTags)
								WriteCallbackOutput (output, scriptBlock, scriptContentNoTags, scriptEntry.Script);
							else
								WriteCallbackOutput (output, scriptBlock, scriptContentWithTags, SerializeScriptBlock (scriptEntry));
							break;
#if NET_3_5
						case RegisteredScriptType.ClientStartupScript:
							if (scriptEntry.AddScriptTags)
								WriteCallbackOutput (output, scriptStartupBlock, scriptContentNoTags, scriptEntry.Script);
							else
								WriteCallbackOutput (output, scriptStartupBlock, scriptContentWithTags, SerializeScriptBlock (scriptEntry));
							break;
#endif
						case RegisteredScriptType.ClientScriptInclude:
							WriteCallbackOutput (output, scriptBlock, scriptPath, scriptEntry.Url);
							break;
						case RegisteredScriptType.OnSubmitStatement:
							WriteCallbackOutput (output, onSubmit, null, scriptEntry.Script);
							break;
					}
				}
			}
		}

		void WriteHiddenFields (HtmlTextWriter output) {
			if (_hiddenFields == null)
				return;
			Hashtable registeredFields = new Hashtable ();
			for (int i = 0; i < _hiddenFields.Count; i++) {
				RegisteredHiddenField field = _hiddenFields [i];
				if (registeredFields.ContainsKey (field.Name))
					continue;
				if (Page == field.Control || HasBeenRendered (field.Control)) {
					registeredFields.Add (field.Name, field);
					WriteCallbackOutput (output, hiddenField, field.Name, field.InitialValue);
				}
			}
		}

		static string SerializeScriptBlock (RegisteredScript scriptEntry) {
			try {
				XmlTextReader reader = new XmlTextReader (new StringReader (scriptEntry.Script));
				while (reader.Read ()) {
					switch (reader.NodeType) {
					case XmlNodeType.Element:
						if (String.Compare ("script", reader.Name, StringComparison.OrdinalIgnoreCase) == 0) {
							Dictionary<string, string> dic = new Dictionary<string, string> ();
							while (reader.MoveToNextAttribute ()) {
								dic.Add (reader.Name, reader.Value);
							}
							reader.MoveToContent ();
							dic.Add ("text", reader.ReadInnerXml ());
							return JavaScriptSerializer.DefaultSerializer.Serialize (dic);
						}
						break;
					default:
						continue;
					}
				}
			}
			catch {
			}
			throw new InvalidOperationException (String.Format ("The script tag registered for type '{0}' and key '{1}' has invalid characters outside of the script tags: {2}. Only properly formatted script tags can be registered.", scriptEntry.Type, scriptEntry.Key, scriptEntry.Script));
		}

		void RenderFormCallback (HtmlTextWriter output, Control container)
		{
			output = ((HtmlTextParser) output).ResponseOutput;
			HtmlForm form = (HtmlForm) container;
			HtmlTextWriter writer = new HtmlDropWriter (output);
			
			if (form.HasControls ()) {
				foreach (Control control in form.Controls)
					control.RenderControl (writer);
			}
		}

		internal class AlternativeHtmlTextWriter : HtmlTextWriter
		{
			readonly HtmlTextWriter _responseOutput;

			public HtmlTextWriter ResponseOutput {
				get { return _responseOutput; }
			}

			public AlternativeHtmlTextWriter (TextWriter writer, HtmlTextWriter responseOutput)
				: base (writer) {
				_responseOutput = responseOutput;
			}
		}

		sealed class HtmlTextParser : AlternativeHtmlTextWriter
		{
			bool _done;

			public Dictionary <string, string> _hiddenFields;

			public Dictionary <string, string> HiddenFields {
				get { return _hiddenFields; }
			}
			
			public HtmlTextParser (HtmlTextWriter responseOutput)
				: this (new TextParser (responseOutput), responseOutput) {
			}

			HtmlTextParser (TextParser parser, HtmlTextWriter responseOutput)
				: base (parser, responseOutput)
			{
				parser.HiddenFieldParsed += new TextParser.TextParserHiddenFieldParsedEventHandler (OnHiddenFieldParsed);
			}
			
			void OnHiddenFieldParsed (TextParser sender, TextParserHiddenFieldParsedEventArgs args)
			{
				if (_hiddenFields == null)
					_hiddenFields = new Dictionary <string, string> ();
				_hiddenFields [args.Name] = args.Value;
			}
			
			public override void WriteAttribute (string name, string value) {
				if (!_done && String.Compare ("action", name, StringComparison.OrdinalIgnoreCase) == 0) {
					_done = true;
					ScriptManager.WriteCallbackOutput (ResponseOutput, formAction, null, value);
					return;
				}
				base.WriteAttribute (name, value);
			}
		}

		sealed class TextParserHiddenFieldParsedEventArgs : EventArgs
		{
			public string Name {
				get;
				private set;
			}

			public string Value {
				get;
				private set;
			}
			
			public TextParserHiddenFieldParsedEventArgs (string name, string value)
			{
				this.Name = name;
				this.Value = value;
			}
		}
		
		sealed class TextParser : TextWriter
		{
			public delegate void TextParserHiddenFieldParsedEventHandler (TextParser sender, TextParserHiddenFieldParsedEventArgs args);

			int _state;
			char _charState = (char) 255;
			const char nullCharState = (char) 255;
			StringBuilder _sb = new StringBuilder ();
			Dictionary<string, string> _currentField;
			string _currentAttribute;
			readonly HtmlTextWriter _responseOutput;
			TextParserHiddenFieldParsedEventHandler _hiddenFieldParsedHandler;
			
			public event TextParserHiddenFieldParsedEventHandler HiddenFieldParsed {
				add { _hiddenFieldParsedHandler = value; }
				remove { _hiddenFieldParsedHandler = null; }
			}
			
			public override Encoding Encoding {
				get { return Encoding.UTF8; }
			}

			public TextParser (HtmlTextWriter responseOutput) {
				_responseOutput = responseOutput;
			}

			public override void Write (char value) {
				switch (_state) {
				case 0:
					ParseBeginTag (value);
					break;
				case 1:
					ParseAttributeName (value);
					break;
				case 2:
					ParseAttributeValue (value);
					break;
				}
			}

			private void ParseAttributeValue (char value) {
				switch (value) {
				case '>':
					ResetState ();
					break;
				case '"':
					_currentField.Add (_currentAttribute, _sb.ToString ());
					_state = 1;
					_sb.Length = 0;
					ProbeWriteOutput ();
					break;
				default:
					_sb.Append (value);
					break;
				}
			}

			private void ParseAttributeName (char value) {
				switch (value) {
				case '>':
					ResetState ();
					break;
				case ' ':
				case '=':
					break;
				case '"':
					_currentAttribute = _sb.ToString ();
					_state = 2;
					_sb.Length = 0;
					break;
				default:
					_sb.Append (value);
					break;
				}
			}

			void ParseBeginTag (char value) {
				switch (_charState) {
				case nullCharState:
					if (value == '<')
						_charState = value;
					break;
				case '<':
					if (value == 'i')
						_charState = value;
					else
						ResetState ();
					break;
				case 'i':
					if (value == 'n')
						_charState = value;
					else
						ResetState ();
					break;
				case 'n':
					if (value == 'p')
						_charState = value;
					else
						ResetState ();
					break;
				case 'p':
					if (value == 'u')
						_charState = value;
					else
						ResetState ();
					break;
				case 'u':
					if (value == 't')
						_charState = value;
					else
						ResetState ();
					break;
				case 't':
					if (value == ' ') {
						_state = 1;
						_currentField = new Dictionary<string, string> ();
					}
					else
						ResetState ();
					break;
				}
			}

			private void ResetState () {
				_charState = nullCharState;
				_state = 0;
				_sb.Length = 0;
			}

			private void ProbeWriteOutput () {
				if (!_currentField.ContainsKey ("name"))
					return;
				if (!_currentField.ContainsKey ("value"))
					return;

				string value = _currentField ["value"];
				if (value == null)
					return;

				if (_hiddenFieldParsedHandler != null)
					_hiddenFieldParsedHandler (this, new TextParserHiddenFieldParsedEventArgs (_currentField ["name"], HttpUtility.HtmlDecode (value)));
				else
					ScriptManager.WriteCallbackOutput (_responseOutput, hiddenField, _currentField ["name"], HttpUtility.HtmlDecode (value));
			}
		}

		sealed class HtmlDropWriter : AlternativeHtmlTextWriter
		{
			public HtmlDropWriter (HtmlTextWriter responseOutput)
				: base (new DropWriter (), responseOutput) {
			}
		}

		sealed class DropWriter : TextWriter
		{
			public override Encoding Encoding {
				get { return Encoding.UTF8; }
			}
		}

		sealed class CultureInfoSerializer
		{
			readonly CultureInfo _ci;
			public CultureInfoSerializer (CultureInfo ci) {
				if (ci == null)
					throw new ArgumentNullException ("ci");
				_ci = ci;
			}

			public string name {
				get { return _ci.Name; }
			}

			public NumberFormatInfo numberFormat {
				get { return _ci.NumberFormat; }
			}

			public DateTimeFormatInfo dateTimeFormat {
				get { return _ci.DateTimeFormat; }
			}			
		}

		sealed class ScriptReferenceEntry
		{
			readonly Control _control;
			readonly ScriptReference _scriptReference;
			readonly bool _loadBeforeUI;

			public Control Control { get { return _control; } }
			public ScriptReference ScriptReference { get { return _scriptReference; } }
			public bool LoadScriptsBeforeUI { get { return _loadBeforeUI; } }

			public ScriptReferenceEntry (Control control, ScriptReference scriptReference, bool loadBeforeUI) {
				_control = control;
				_scriptReference = scriptReference;
				_loadBeforeUI = loadBeforeUI;
			}
		}

		sealed class DataItemEntry
		{
			readonly string _dataItem;
			readonly bool _isJsonSerialized;

			public string DataItem { get { return _dataItem; } }
			public bool IsJsonSerialized { get { return _isJsonSerialized; } }

			public DataItemEntry (string dataItem, bool isJsonSerialized) {
				_dataItem = dataItem;
				_isJsonSerialized = isJsonSerialized;
			}
		}

		internal void RegisterProxy (ScriptManagerProxy scriptManagerProxy) {
			if (_proxies == null)
				_proxies = new List<ScriptManagerProxy> ();

			_proxies.Add (scriptManagerProxy);
		}

		#region IScriptManager Members

		void IScriptManager.RegisterOnSubmitStatementExternal (Control control, Type type, string key, string script) {
			RegisterOnSubmitStatement (control, type, key, script);
		}

		void IScriptManager.RegisterExpandoAttributeExternal (Control control, string controlId, string attributeName, string attributeValue, bool encode) {
			RegisterExpandoAttribute (control, controlId, attributeName, attributeValue, encode);
		}

		void IScriptManager.RegisterHiddenFieldExternal (Control control, string hiddenFieldName, string hiddenFieldInitialValue) {
			RegisterHiddenField (control, hiddenFieldName, hiddenFieldInitialValue);
		}

		void IScriptManager.RegisterStartupScriptExternal (Control control, Type type, string key, string script, bool addScriptTags) {
			RegisterStartupScript (control, type, key, script, addScriptTags);
		}

		void IScriptManager.RegisterArrayDeclarationExternal (Control control, string arrayName, string arrayValue) {
			RegisterArrayDeclaration (control, arrayName, arrayValue);
		}

		void IScriptManager.RegisterClientScriptBlockExternal (Control control, Type type, string key, string script, bool addScriptTags) {
			RegisterClientScriptBlock (control, type, key, script, addScriptTags);
		}

		void IScriptManager.RegisterClientScriptIncludeExternal (Control control, Type type, string key, string url) {
			RegisterClientScriptInclude (control, type, key, url);
		}

		void IScriptManager.RegisterClientScriptResourceExternal (Control control, Type type, string resourceName) {
			RegisterClientScriptResource (control, type, resourceName);
		}

		#endregion
	}
}
