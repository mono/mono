//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
using System.Web.Util;
using System.IO;
using vmw.@internal.io;
using vmw.common;

using javax.servlet;

namespace Mainsoft.Web
{
	internal static class J2EEUtils
	{
		public static string GetInitParameterByHierarchy(ServletConfig config, string name)
		{
			if (config == null)
				throw new ArgumentNullException("config");

			string value = config.getInitParameter(name);
			if (value != null)
				return value;

			return config.getServletContext().getInitParameter(name);
		}

		public static string GetApplicationRealPath (ServletConfig config) {
			return GetApplicationRealPath (config, "/");
		}

		public static string GetApplicationRealPath (ServletConfig config, string appVirtualPath)
		{
			string realFs = GetInitParameterByHierarchy (config, J2EEConsts.FILESYSTEM_ACCESS);
			if (realFs == null || realFs == J2EEConsts.ACCESS_FULL) {
				try {
					string realPath = config.getServletContext ().getRealPath (appVirtualPath);
					if (!String.IsNullOrEmpty (realPath)) {
						if (!String.IsNullOrEmpty (appVirtualPath) &&
							appVirtualPath [appVirtualPath.Length - 1] == '/')
							if (realPath [realPath.Length - 1] != Path.DirectorySeparatorChar)
								realPath += Path.DirectorySeparatorChar;

						return realPath;
					}
				}
				catch (Exception e) {
#if TRACE
					Console.WriteLine (e.ToString());
#endif
				}
			}
			return IAppDomainConfig.WAR_ROOT_SYMBOL + appVirtualPath;
		}

		public static string GetApplicationPhysicalPath (ServletConfig config) {
			string path = String.Empty;

			ServletContext context = config.getServletContext ();
			string appDir = GetInitParameterByHierarchy (config, IAppDomainConfig.APP_DIR_NAME);
			if (appDir != null) {
				try {
					if (Directory.Exists(appDir))
						path = appDir;
				}
				catch (Exception e) {
#if TRACE
					Console.WriteLine (e.Message + appDir + "is invalid or unaccessible." +
						" If " + appDir + " really exists, check your security permissions");
#endif
				}
			}
			if (path.Length == 0)
				path = GetApplicationRealPath (config);

			return path;
		}
	}
}
