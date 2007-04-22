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

namespace System.Web.UI
{
	public partial class Page
	{
		const string PageNamespaceKey = "__PAGENAMESPACE";
		const string RenderPageMark = "vmw.render.page=";
		const string ActionPageMark = "vmw.action.page=";
		static readonly string NextActionPageKey = PortletInternalUtils.NextActionPage;
		static readonly string NextRenderPageKey = PortletInternalUtils.NextRenderPage;

		bool _emptyPortletNamespace = false;
		string _PortletNamespace = null;
		bool _renderResponseInit = false;
		IPortletRenderResponse _renderResponse = null;

		internal string PortletNamespace
		{
			get {
				if (_emptyPortletNamespace)
					return null;

				if (_PortletNamespace == null) {
					IPortletResponse portletResponse = null;
					if (Context != null) {
						string usePortletNamespace = J2EEUtils.GetInitParameterByHierarchy (Context.Servlet.getServletConfig (), "mainsoft.use.portlet.namespace");
						if (usePortletNamespace == null || Boolean.Parse(usePortletNamespace))
							portletResponse = Context.ServletResponse as IPortletResponse;
					}
					if (portletResponse != null)
						_PortletNamespace = portletResponse.getNamespace ();
					else if (_requestValueCollection != null && _requestValueCollection [PageNamespaceKey] != null)
						_PortletNamespace = _requestValueCollection [PageNamespaceKey];
						
					_emptyPortletNamespace = _PortletNamespace == null;
				}
				return _PortletNamespace;
			}
		}

		internal string theForm {
			get {
				return "theForm" + PortletNamespace;
			}
		}

		internal bool IsPortletRender
		{
			get {
				return RenderResponse != null;
			}
		}

		internal IPortletRenderResponse RenderResponse
		{
			get {
				if (!_renderResponseInit) {
					_renderResponse = Context.ServletResponse as IPortletRenderResponse;
					_renderResponseInit = true;
				}
				return _renderResponse;
			}
		}

		public string CreateRenderUrl (string url)
		{
			if (RenderResponse != null)
				return RenderResponse.createRenderURL (url);
			if (PortletNamespace == null)
				return url;

			string internalUrl = RemoveAppPathIfInternal (url);
			if (internalUrl == null)
				return url;

			PostBackOptions options = new PostBackOptions (this);
			options.ActionUrl = RenderPageMark + internalUrl;
			options.RequiresJavaScriptProtocol = true;
			return ClientScript.GetPostBackEventReference (options);
		}

		public string CreateActionUrl (string url)
		{
			if (url.StartsWith (RenderPageMark, StringComparison.Ordinal) || url.StartsWith (ActionPageMark, StringComparison.Ordinal))
				return url;

			if (RenderResponse != null)
				return RenderResponse.createActionURL (url);
			if (PortletNamespace == null)
				return url;

			Uri requestUrl = Request.Url;
			string internalUrl = RemoveAppPathIfInternal (url);
			if (internalUrl == null)
				return url;

			return ActionPageMark + internalUrl;
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
			}

			// Stop processing only if we are handling processAction. If we
			// are handling a postback from render then fall through.
			return req.processActionOnly ();
		}
	}
}
