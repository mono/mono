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

namespace System.Web.UI
{
	public partial class Page
	{
		bool _emptyPortletNamespace = false;
		string _PortletNamespace = null;

		internal string PortletNamespace
		{
			get {
				if (_emptyPortletNamespace)
					return null;

				if (_PortletNamespace == null) {
					IPortletResponse portletResponse = Context.ServletResponse as IPortletResponse;
					if (portletResponse != null)
						_PortletNamespace = portletResponse.getNamespace ();
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

		internal bool SaveViewStateForNextPortletRender ()
		{
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
