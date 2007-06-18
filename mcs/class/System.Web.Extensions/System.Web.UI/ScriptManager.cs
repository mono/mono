//
// ScriptManager.cs
//
// Author:
//   Igor Zelmanovich <igorz@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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

namespace System.Web.UI
{
	[ParseChildrenAttribute (true)]
	[DefaultPropertyAttribute ("Scripts")]
	[DesignerAttribute ("System.Web.UI.Design.ScriptManagerDesigner, System.Web.Extensions.Design, Version=1.0.61025.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
	[NonVisualControlAttribute]
	[PersistChildrenAttribute (false)]
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class ScriptManager : Control, IPostBackDataHandler
	{
		// the keywords are used in fomatting async response
		const string updatePanel = "updatePanel";
		const string hiddenField = "hiddenField";
		const string arrayDeclaration = "arrayDeclaration";
		const string scriptBlock = "scriptBlock";
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

		int _asyncPostBackTimeout = 90;
		List<Control> _asyncPostBackControls;
		List<Control> _postBackControls;
		ScriptReferenceCollection _scripts;
		bool _isInAsyncPostBack;
		string _asyncPostBackSourceElementID;
		ScriptMode _scriptMode = ScriptMode.Auto;
		HtmlTextWriter _output;
		
		[DefaultValue (true)]
		[Category ("Behavior")]
		public bool AllowCustomErrorsRedirect {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Category ("Behavior")]
		[DefaultValue ("")]
		public string AsyncPostBackErrorMessage {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		public string AsyncPostBackSourceElementID {
			get {
				if(_asyncPostBackSourceElementID==null)
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
				throw new NotImplementedException ();
			}
		}

		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool EnablePageMethods {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue (true)]
		[Category ("Behavior")]
		public bool EnablePartialRendering {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue (false)]
		[Category ("Behavior")]
		public bool EnableScriptGlobalization {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool EnableScriptLocalization {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		public bool IsDebuggingEnabled {
			get {
				DeploymentSection deployment = (DeploymentSection) WebConfigurationManager.GetSection ("system.web/deployment");
				if (deployment.Retail)
					return false;

				CompilationSection compilation = (CompilationSection) WebConfigurationManager.GetSection ("system.web/compilation");
				if (!compilation.Debug && (ScriptMode == ScriptMode.Auto || ScriptMode == ScriptMode.Inherit))
					return false;

				if (ScriptMode == ScriptMode.Release)
					return false;

				return true;
			}
		}

		[Browsable (false)]
		public bool IsInAsyncPostBack {
			get {
				return _isInAsyncPostBack;
			}
		}

		[Category ("Behavior")]
		[DefaultValue (true)]
		public bool LoadScriptsBeforeUI {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Category ("Behavior")]
		[MergableProperty (false)]
		public ProfileServiceManager ProfileService {
			get {
				throw new NotImplementedException ();
			}
		}

		[Category ("Behavior")]
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
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
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

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[MergableProperty (false)]
		[Category ("Behavior")]
		public ServiceReferenceCollection Services {
			get {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue (true)]
		[Browsable (false)]
		public bool SupportsPartialRendering {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
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

		public static ScriptManager GetCurrent (Page page)
		{
			HttpContext ctx = HttpContext.Current;
			if (ctx == null)
				return null;

			return (ScriptManager) ctx.Items [page];
		}
		
		static void SetCurrent (Page page, ScriptManager instance) {
			HttpContext ctx = HttpContext.Current;
			ctx.Items [page] = instance;
		}

		protected virtual bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			_isInAsyncPostBack = true;
			_asyncPostBackSourceElementID = postCollection [postDataKey];
			return false;
		}

		protected internal virtual void OnAsyncPostBackError (AsyncPostBackErrorEventArgs e)
		{
			if (AsyncPostBackError != null)
				AsyncPostBackError (this, e);
		}

		protected override void OnInit (EventArgs e)
		{
			base.OnInit (e);

			if (GetCurrent (Page) != null)
				throw new InvalidOperationException ("Only one instance of a ScriptManager can be added to the page.");

			SetCurrent (Page, this);
		}

		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			
			if (IsInAsyncPostBack) {
				Page.SetRenderMethodDelegate (RenderPageCallback);
			}
			
			// Register Scripts
			foreach (ScriptReference script in GetScriptReferences ()) {
				OnResolveScriptReference (new ScriptReferenceEventArgs (script));
				RegisterScriptReference (script);
			}

			// Register startup script
			RegisterStartupScript (this, typeof (ScriptManager), "Sys.Application.initialize();", "Sys.Application.initialize();", true);
		}

		IEnumerable GetScriptReferences () {
			ScriptReference script;

			script = new ScriptReference ("MicrosoftAjax.js", String.Empty);
			yield return script;

			script = new ScriptReference ("MicrosoftAjaxWebForms.js", String.Empty);
			yield return script;

			if (_scripts != null && _scripts.Count > 0) {
				for (int i = 0; i < _scripts.Count; i++) {
					yield return _scripts [i];
				}
			}
		}

		protected virtual void OnResolveScriptReference (ScriptReferenceEventArgs e)
		{
			if (ResolveScriptReference != null)
				ResolveScriptReference (this, e);
		}

		protected virtual void RaisePostDataChangedEvent ()
		{
			throw new NotImplementedException ();
		}

		public static void RegisterArrayDeclaration (Control control, string arrayName, string arrayValue)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterArrayDeclaration (Page page, string arrayName, string arrayValue)
		{
			throw new NotImplementedException ();
		}

		public void RegisterAsyncPostBackControl (Control control)
		{
			if(control==null)
				return;

			if (_asyncPostBackControls == null)
				_asyncPostBackControls = new List<Control> ();

			if (_asyncPostBackControls.Contains (control))
				return;

			_asyncPostBackControls.Add (control);
		}

		public static void RegisterClientScriptBlock (Control control, Type type, string key, string script, bool addScriptTags)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterClientScriptBlock (Page page, Type type, string key, string script, bool addScriptTags)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterClientScriptInclude (Control control, Type type, string key, string url)
		{
			RegisterClientScriptInclude (control.Page, type, key, url);
		}

		public static void RegisterClientScriptInclude (Page page, Type type, string key, string url)
		{
			page.ClientScript.RegisterClientScriptInclude (type, key, url);
		}

		public static void RegisterClientScriptResource (Control control, Type type, string resourceName)
		{
			RegisterClientScriptResource (control.Page, type, resourceName);
		}

		public static void RegisterClientScriptResource (Page page, Type type, string resourceName)
		{
			page.ClientScript.RegisterClientScriptResource (type, resourceName);
		}

		void RegisterScriptReference (ScriptReference script) {

			// TODO: consider 'retail' attribute of the 'deployment' configuration element in Web.config, 
			// IsDebuggingEnabled and ScriptMode properties to determine whether to render debug scripts.

			string url;
			if (!String.IsNullOrEmpty (script.Path)) {
				url = script.Path;
			}
			else if (!String.IsNullOrEmpty (script.Name)) {
				Assembly assembly;
				if (String.IsNullOrEmpty (script.Assembly))
					assembly = typeof (ScriptManager).Assembly;
				else
					assembly = Assembly.Load (script.Assembly);
				url = ScriptResourceHandler.GetResourceUrl (assembly, script.Name);
			}
			else {
				throw new InvalidOperationException ("Name and Path cannot both be empty.");
			}

			RegisterClientScriptInclude (this, typeof (ScriptManager), url, url);
		}

		public void RegisterDataItem (Control control, string dataItem)
		{
			throw new NotImplementedException ();
		}

		public void RegisterDataItem (Control control, string dataItem, bool isJsonSerialized)
		{
			throw new NotImplementedException ();
		}

		public void RegisterDispose (Control control, string disposeScript)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterExpandoAttribute (Control control, string controlId, string attributeName, string attributeValue, bool encode)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterHiddenField (Control control, string hiddenFieldName, string hiddenFieldInitialValue)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterHiddenField (Page page, string hiddenFieldName, string hiddenFieldInitialValue)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterOnSubmitStatement (Control control, Type type, string key, string script)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterOnSubmitStatement (Page page, Type type, string key, string script)
		{
			throw new NotImplementedException ();
		}

		public void RegisterPostBackControl (Control control)
		{
			if (control == null)
				return;

			if (_postBackControls == null)
				_postBackControls = new List<Control> ();

			if (_postBackControls.Contains (control))
				return;

			_postBackControls.Add (control);
		}

		public void RegisterScriptDescriptors (IExtenderControl extenderControl)
		{
			throw new NotImplementedException ();
		}

		public void RegisterScriptDescriptors (IScriptControl scriptControl)
		{
			throw new NotImplementedException ();
		}

		public static void RegisterStartupScript (Control control, Type type, string key, string script, bool addScriptTags)
		{
			RegisterStartupScript (control.Page, type, key, script, addScriptTags);
		}

		public static void RegisterStartupScript (Page page, Type type, string key, string script, bool addScriptTags)
		{
			page.ClientScript.RegisterStartupScript (type, key, script, addScriptTags);
		}

		protected override void Render (HtmlTextWriter writer)
		{
			// MSDN: This method is used by control developers to extend the ScriptManager control. 
			// Notes to Inheritors: 
			// When overriding this method, call the base Render(HtmlTextWriter) method 
			// so that PageRequestManager is rendered on the page.
			writer.WriteLine ("<script type=\"text/javascript\">");
			writer.WriteLine ("//<![CDATA[");
			writer.WriteLine ("Sys.WebForms.PageRequestManager._initialize('{0}', document.getElementById('{1}'));", UniqueID, Page.Form.ClientID);
			writer.WriteLine ("Sys.WebForms.PageRequestManager.getInstance()._updateControls([{0}], [{1}], [{2}], {3});", null, FormatListIDs (_asyncPostBackControls), FormatListIDs (_postBackControls), AsyncPostBackTimeout);
			writer.WriteLine ("//]]");
			writer.WriteLine ("</script>");
			base.Render (writer);
		}

		static string FormatListIDs(List<Control> list)
		{
			if (list == null || list.Count == 0)
				return null;

			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < list.Count; i++) {
				sb.AppendFormat ("'{0}',", list [i].UniqueID);
			}
			if (sb.Length > 0)
				sb.Length--;

			return sb.ToString ();
		}

		public void SetFocus (Control control)
		{
			throw new NotImplementedException ();
		}

		public void SetFocus (string clientID)
		{
			throw new NotImplementedException ();
		}

		#region IPostBackDataHandler Members

		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}

		#endregion

		void RenderPageCallback (HtmlTextWriter output, Control container) {
			Page page = (Page) container;
			_output = output;

			page.Form.SetRenderMethodDelegate (RenderFormCallback);
			page.Form.RenderControl (output);
		}

		void RenderFormCallback (HtmlTextWriter output, Control container) {
			HtmlForm form = (HtmlForm) container;
		}
	}
}
