//
// System.Web.UI.ControlS.jvm.cs
//
// Authors:
//   Eyal Alaluf (eyala@mainsoft.com)
//
// (C) 2006 Mainsoft Co. (http://www.mainsoft.com)
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

using vmw.@internal.j2ee;
using System.Web.Hosting;

namespace System.Web.UI
{
	public partial class Control
	{
		bool _emptyPortletNamespace = false;
		string _PortletNamespace = null;

		internal bool IsPortletRender
		{
			get {
				return GetRenderResponse() != null;
			}
		}

		internal string PortletNamespace
		{
			get {
				if (_emptyPortletNamespace)
					return null;

				if (_PortletNamespace == null) {
					IPortletResponse portletResponse = GetRenderResponse ();
					if (portletResponse != null)
						_PortletNamespace = portletResponse.getNamespace ();
					_emptyPortletNamespace = _PortletNamespace == null;
				}
				return _PortletNamespace;
			}
		}

		// For J2EE Portal we need to use the portlet namespace when we generate control IDs.
		string GetDefaultName ()
		{
			string defaultName;
			if (defaultNumberID > 99) {
				defaultName = "_ctl" + defaultNumberID++;
			} else {
				defaultName = defaultNameArray [defaultNumberID++];
			}

			if (this != _page)
				return defaultName;

			return PortletNamespace + defaultName;
		}

		// Add a variant for specifying use of portlet resolveRenderUrl
		internal string ResolveUrl (string relativeUrl, bool usePortletRenderResolve)
		{
			relativeUrl = ResolveUrl (relativeUrl);
			if (usePortletRenderResolve) {
				IPortletRenderResponse resp = GetRenderResponse ();
				if (resp != null)
					relativeUrl = resp.createRenderURL (relativeUrl);
			}
			return relativeUrl;
		}

		internal string ResolveClientUrl (string relativeUrl, bool usePortletRenderResolve)
		{
			relativeUrl = ResolveClientUrl (relativeUrl);
			if (usePortletRenderResolve) {
				IPortletRenderResponse resp = GetRenderResponse ();
				if (resp != null)
					relativeUrl = resp.createRenderURL (relativeUrl);
			}
			return relativeUrl;
		}

		internal IPortletRenderResponse GetRenderResponse ()
		{
			return Context.ServletResponse as IPortletRenderResponse;
		}
	}
}
