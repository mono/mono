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
using System.Text;
using javax.faces.context;

namespace System.Web.UI
{
	public partial class Control
	{
		public virtual string TemplateSourceDirectory
		{
			get
			{
				if (_templateSourceDirectory == null) {
					_templateSourceDirectory = VirtualPathUtility.ToAbsolute (AppRelativeTemplateSourceDirectory, false);

					if (_templateSourceDirectory.Length > 1 &&
						_templateSourceDirectory [_templateSourceDirectory.Length - 1] == '/')
						_templateSourceDirectory = _templateSourceDirectory.Substring (0, _templateSourceDirectory.Length - 1);
				}

				return _templateSourceDirectory;
			}
		}

		string ResolveAppRelativeFromFullPath (string url) {
			Uri uri = new Uri (url);
			if (String.Compare (uri.Scheme, Page.Request.Url.Scheme, StringComparison.OrdinalIgnoreCase) == 0 &&
				String.Compare (uri.Host, Page.Request.Url.Host, StringComparison.OrdinalIgnoreCase) == 0 &&
				uri.Port == Page.Request.Url.Port)
				return VirtualPathUtility.ToAppRelative (uri.PathAndQuery);
			return url;
		}

		internal string CreateActionUrl (string url) {
			FacesContext faces = getFacesContext ();
			if (faces == null)
				return url;

			url = Asp2Jsf (url);

			return faces.getApplication ().getViewHandler ().getActionURL (faces, url);
		}

		string Asp2Jsf (string url) {
			if (VirtualPathUtility.IsAbsolute (url))
				url = VirtualPathUtility.ToAppRelative (url);

			if (VirtualPathUtility.IsAppRelative (url)) {
				url = url.Substring (1);
				return url.Length == 0 ? "/" : url;
			}
			return url;
		}

		internal string ResolveClientUrl (string relativeUrl, bool usePortletRenderResolve) {
			if (usePortletRenderResolve)
				return ResolveClientUrl (relativeUrl);
			else
				return ResolveUrl (relativeUrl);
		}

		internal bool IsLoaded {
			get { return (stateMask & LOADED) != 0; }
		}

		internal bool IsPrerendered {
			get { return (stateMask & PRERENDERED) != 0; }
		}
	}
}
