//
// System.Web.UI.Page.jvm.cs
//
// Authors:
//   Eyal Alaluf (eyala@mainsoft.com)
//
// (C) 2006 Mainsoft Co. (http://www.mainsoft.com)
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

using vmw.@internal.j2ee;
using javax.servlet.http;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.Hosting;
using System.Web.J2EE;
using System.ComponentModel;
using System.IO;
using javax.faces.context;
using javax.faces.render;
using javax.servlet;
using javax.faces;
using javax.faces.application;

namespace System.Web.UI
{
	public partial class Page
	{
		internal const string NamespaceKey = "__NAMESPACE";
		const string PageNamespaceKey = "__PAGENAMESPACE";
		const string RenderPageMark = "vmw.render.page=";
		const string ActionPageMark = "vmw.action.page=";
		static readonly string NextActionPageKey = PortletInternalUtils.NextActionPage;
		static readonly string NextRenderPageKey = PortletInternalUtils.NextRenderPage;

		bool _emptyPortletNamespace = false;
		string _PortletNamespace = null;
		bool _renderResponseInit = false;
		IPortletRenderResponse _renderResponse = null;
		StateManager.SerializedView _facesSerializedView;


		internal string PortletNamespace
		{
			get {
				return Context.PortletNamespace;
			}
		}

		internal string theForm {
			get {
				return "theForm" + PortletNamespace;
			}
		}
		
		bool _isMultiForm = false;
		bool _isMultiFormInited = false;

		internal bool IsMultiForm {
			get {
				if (!_isMultiFormInited) {
					Mainsoft.Web.Configuration.PagesSection pageSection = (Mainsoft.Web.Configuration.PagesSection) System.Web.Configuration.WebConfigurationManager.GetSection ("mainsoft.web/pages");
					if (pageSection != null)
						_isMultiForm = pageSection.MultiForm;

					_isMultiFormInited = true;
				}
				return _isMultiForm;
			}
		}

		internal bool IsPortletRender
		{
			get {
				return RenderResponse != null;
			}
		}

		internal bool IsGetBack {
			get {
				return IsPostBack && Context.IsPortletRequest && !Context.IsActionRequest;
			}
		}

		internal IPortletRenderResponse RenderResponse
		{
			get {
				if (!_renderResponseInit)
					if (Context != null) {
						_renderResponse = Context.ServletResponse as IPortletRenderResponse;
						_renderResponseInit = true;
					}
				return _renderResponse;
			}
		}

		public string CreateRenderUrl (string url)
		{
			FacesContext faces = getFacesContext ();
			return faces != null ? faces.getExternalContext ().encodeResourceURL (url) : url;
		}

		public string CreateActionUrl (string url)
		{
			FacesContext faces = getFacesContext ();
			if (faces == null)
				return url;

			//kostat: handle QueryString!
			Application application = faces.getApplication ();
			ViewHandler viewHandler = application.getViewHandler ();
			String viewId = faces.getViewRoot ().getViewId ();
			return viewHandler.getActionURL (faces, viewId);
		}

		private string RemoveAppPathIfInternal (string url)
		{
			Uri reqUrl = Request.Url;
			string appPath = Request.ApplicationPath;
			string currPage = Request.CurrentExecutionFilePath;
			if (currPage.StartsWith (appPath, StringComparison.InvariantCultureIgnoreCase))
				currPage = currPage.Substring (appPath.Length);
			return PortletInternalUtils.mapPathIfInternal (url, reqUrl.Host, reqUrl.Port, reqUrl.Scheme, appPath, currPage);
		}

		internal bool OnSaveStateCompleteForPortlet ()
		{
			if (PortletNamespace != null) {
				ClientScript.RegisterHiddenField (PageNamespaceKey, PortletNamespace);
				ClientScript.RegisterHiddenField (NextActionPageKey, "");
				ClientScript.RegisterHiddenField (NextRenderPageKey, "");
			}

			IPortletActionResponse resp = Context.ServletResponse as IPortletActionResponse;
			IPortletActionRequest req = Context.ServletRequest as IPortletActionRequest;
			if (req == null)
				return false;

			// When redirecting don't save the page viewstate and hidden fields
			if (resp.isRedirected ())
				return true;

			if (IsPostBack && 0 == String.Compare (Request.HttpMethod, "POST", true, CultureInfo.InvariantCulture)) {
				resp.setRenderParameter ("__VIEWSTATE", GetSavedViewState ());
				if (ClientScript.hiddenFields != null)
					foreach (string key in ClientScript.hiddenFields.Keys)
						resp.setRenderParameter (key, (string) ClientScript.hiddenFields [key]);

				if (is_validated && Validators.Count > 0) {
					string validatorsState = GetValidatorsState ();
#if DEBUG
					Console.WriteLine ("__VALIDATORSSTATE: " + validatorsState);
#endif
					if (!String.IsNullOrEmpty (validatorsState))
						resp.setRenderParameter ("__VALIDATORSSTATE", validatorsState);
				}
			}

			// Stop processing only if we are handling processAction. If we
			// are handling a postback from render then fall through.
			return req.processActionOnly ();
		}

		string GetValidatorsState () {
			bool [] validatorsState = new bool [Validators.Count];
			bool isValid = true;
			for (int i = 0; i < Validators.Count; i++) {
				IValidator val = Validators [i];
				if (!val.IsValid)
					isValid = false;
				else
					validatorsState [i] = true;
			}
			if (isValid)
				return null;

			return GetFormatter ().Serialize (validatorsState);
		}

		void RestoreValidatorsState () {
			string validatorsStateSerialized = Request.Form ["__VALIDATORSSTATE"];
			if (String.IsNullOrEmpty (validatorsStateSerialized))
				return;

			is_validated = true;
			bool [] validatorsState = (bool []) GetFormatter ().Deserialize (validatorsStateSerialized);
			for (int i = 0; i < Math.Min (validatorsState.Length, Validators.Count); i++) {
				IValidator val = Validators [i];
				val.IsValid = validatorsState [i];
			}
		}

		void SetupResponseWriter (TextWriter httpWriter) {
			FacesContext facesContext = getFacesContext ();
			if (facesContext == null)
				return;
			ResponseWriter writer = facesContext.getResponseWriter ();
			if (writer == null) {
				RenderKitFactory renderFactory = (RenderKitFactory) FactoryFinder.getFactory (FactoryFinder.RENDER_KIT_FACTORY);
				RenderKit renderKit = renderFactory.getRenderKit (facesContext,
																 facesContext.getViewRoot ().getRenderKitId ());

				ServletResponse response = (ServletResponse) facesContext.getExternalContext ().getResponse ();

				writer = renderKit.createResponseWriter (new AspNetResponseWriter (httpWriter),
														 response.getContentType (), //TODO: is this the correct content type?
														 response.getCharacterEncoding ());
				facesContext.setResponseWriter (writer);
			}
		}

		internal string EncodeURL (string raw) {
			//kostat: BUGBUG: complete
			return raw;
		}

	}
}
