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

		public static string GetApplicationRealPath (ServletConfig config)
		{
			string realFs = GetInitParameterByHierarchy (config, J2EEConsts.FILESYSTEM_ACCESS);
			if (realFs == null || realFs == J2EEConsts.ACCESS_FULL) {
				try {
					if (Path.IsPathRooted (config.getServletContext ().getRealPath ("")))
						return config.getServletContext ().getRealPath ("").Replace ("\\", "/").TrimEnd ('/');
				}
				catch (ArgumentException e) {
					Console.WriteLine (e.Message);
				}
				catch (Exception e) {
					Console.WriteLine (e.Message);
				}
			}
			return IAppDomainConfig.WAR_ROOT_SYMBOL;
		}

		public static string GetApplicationPhysicalPath (ServletConfig config) {
			string path = "";
			ServletContext context = config.getServletContext ();
			string appDir = GetInitParameterByHierarchy (config, IAppDomainConfig.APP_DIR_NAME);
			//			Console.WriteLine("appdir = {0}", appDir);
			if (appDir != null) {
				try {
					java.io.File f = new java.io.File (appDir);
					if (f.exists ()) {
						//						Console.WriteLine("Physical path= {0}", appDir);
						path = appDir;
					}
				}
				catch (Exception e) {
					Console.WriteLine (e.Message + appDir + "is invalid or unaccessible." +
						" If " + appDir + " really exists, check your security permissions");
				};
			}
			if (path == "") {
				path = GetApplicationRealPath (config);
			}

			if (!path.EndsWith ("/") && !path.EndsWith ("\\"))
				path += "/";

			//			Console.WriteLine("Physical path= {0}", path); 
			return path;
		}
	}
}
