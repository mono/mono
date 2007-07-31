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


		// Add a variant for specifying use of portlet resolveRenderUrl
		internal string ResolveUrl (string relativeUrl, bool usePortletRenderResolve)
		{
			relativeUrl = ResolveUrl (relativeUrl);
			if (usePortletRenderResolve && Page != null)
				relativeUrl = Page.CreateRenderUrl (relativeUrl);
			return relativeUrl;
		}

		internal string ResolveClientUrl (string relativeUrl, bool usePortletRenderResolve)
		{
			relativeUrl = ResolveClientUrl (relativeUrl);
			if (usePortletRenderResolve && Page != null)
				relativeUrl = Page.CreateRenderUrl (relativeUrl);
			return relativeUrl;
		}
	}
}
