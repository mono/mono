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
		private string _templateSourceDir;

		public virtual string TemplateSourceDirectory
		{
			get
			{
				int location = 0;
				if (_templateSourceDir == null) {
					string tempSrcDir = _appRelativeTemplateSourceDirectory;
					if (tempSrcDir == null && Parent != null)
						tempSrcDir = Parent.TemplateSourceDirectory;
					if (tempSrcDir != null && tempSrcDir.Length > 1) {
						location = tempSrcDir.IndexOf ('/', 1);
						if (location != -1)
							tempSrcDir = tempSrcDir.Substring (location + 1);
						else
							tempSrcDir = string.Empty;
					}
					string answer = HttpRuntime.AppDomainAppVirtualPath;
					if (tempSrcDir == null)
						tempSrcDir = "";

					if (tempSrcDir.Length > 0 && tempSrcDir [tempSrcDir.Length - 1] == '/')
						tempSrcDir = tempSrcDir.Substring (0, tempSrcDir.Length - 1);

					if (tempSrcDir.StartsWith ("/") || tempSrcDir.Length == 0)
						_templateSourceDir = answer + tempSrcDir;
					else
						_templateSourceDir = answer + "/" + tempSrcDir;
				}
				return _templateSourceDir;
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
