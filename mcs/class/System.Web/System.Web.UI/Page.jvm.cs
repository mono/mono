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
using System.Web.Hosting;

namespace System.Web.UI
{
	public partial class Page
	{
		internal string PostBackFunctionName {
			get {
#if LATER // Enable when we fix the jscripts not to reference __doPostBack.
				IPortletRenderResponse resp = GetRenderResponse();
				if (resp != null)
					return "__doPostBack_" + resp.getNamespace();
#endif
				return "__doPostBack";
			}
		}

		// For J2EE portlets we load the view state from the render parameters
		WebROCollection LoadViewStateForPortlet(WebROCollection coll)
		{
			IPortletRenderRequest renderRequest = Context.ServletRequest as IPortletRenderRequest;
			if (renderRequest != null && (coll == null || coll ["__VIEWSTATE"] == null)) {
				string mode = renderRequest.getPortletMode();
				string viewstate = Context.ServletRequest.getParameter("vmw.viewstate." + mode);
				if (viewstate != null) {
					if (coll == null)
						coll = new WebROCollection();
					else 
						coll.Unprotect();
					coll["__VIEWSTATE"] = viewstate;
					coll.Protect();
				}
			}
			return coll;
		}

		internal bool SaveViewStateForNextPortletRender()
		{
			IPortletActionResponse resp = Context.ServletResponse as IPortletActionResponse;
			IPortletActionRequest req = Context.ServletRequest as IPortletActionRequest;
			if (req == null)
				return false;

			if (IsPostBack && String.Compare (Request.HttpMethod, "POST", true) == 0 && !resp.isRedirected())
				resp.setRenderParameter("vmw.viewstate." + req.getPortletMode(), GetSavedViewState());

			// Stop processing only if we are handling processAction. If we
			// are handling a postback from render then fall through.
			return req.processActionOnly() || resp.isRedirected();
		}
	}
}
