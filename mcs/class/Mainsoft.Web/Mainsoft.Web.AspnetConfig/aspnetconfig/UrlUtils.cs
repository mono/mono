//
// System.Web.UrlUtils.cs 
//
// Authors:
//	Gonzalo Paniagua (gonzalo@ximian.com)
//      Jackson Harper   (jackson@ximian.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Web.SessionState;
using System.Web;
using System;

namespace Mainsoft.Web.Util {
	
	internal class UrlUtils {

		// appRoot + SessionID + vpath
		internal static string InsertSessionId (string id, string path)
		{
			string dir = GetDirectory (path);
			if (!dir.EndsWith ("/"))
				dir += "/";

			string appvpath = HttpRuntime.AppDomainAppVirtualPath;
			if (!appvpath.EndsWith ("/"))
				appvpath += "/";

			if (path.StartsWith (appvpath))
				path = path.Substring (appvpath.Length);

			if (path [0] == '/')
				path = path.Length > 1 ? path.Substring (1) : "";

			return Canonic (appvpath + "(" + id + ")/" + path);
		}
#if false
		internal static string GetSessionId (string path)
		{
			string appvpath = HttpRuntime.AppDomainAppVirtualPath;
			if (path.Length <= appvpath.Length)
				return null;

			path = path.Substring (appvpath.Length);
			if (path.Length == 0 || path [0] != '/')
				path = '/' + path;

			int len = path.Length;
			if ((len < SessionId.IdLength + 3) || (path [1] != '(') ||
			    (path [SessionId.IdLength + 2] != ')'))
				return null;

			return path.Substring (2, SessionId.IdLength);
		}
#endif
		internal static string RemoveSessionId (string base_path, string file_path)
		{
			// Caller did a GetSessionId first
			int idx = base_path.IndexOf ("/(");
			string dir = base_path.Substring (0, idx + 1);
			if (!dir.EndsWith ("/"))
				dir += "/";

			idx = base_path.IndexOf (")/");
			if (idx != -1 && base_path.Length > idx + 2) {
				string dir2 = base_path.Substring (idx + 2);
				if (!dir2.EndsWith ("/"))
					dir2 += "/";

				dir += dir2;
			}

			return Canonic (dir + GetFile (file_path));
		}

		public static string Combine (string basePath, string relPath)
		{
			if (relPath == null)
				throw new ArgumentNullException ("relPath");

			int rlength = relPath.Length;
			if (rlength == 0)
				return "";

			relPath = relPath.Replace ("\\", "/");
			if (IsRooted (relPath))
				return Canonic (relPath);

			char first = relPath [0];
			if (rlength < 3 || first == '~' || first == '/' || first == '\\') {
				if (basePath == null || (basePath.Length == 1 && basePath [0] == '/'))
					basePath = String.Empty;

				string slash = (first == '/') ? "" : "/";
				if (first == '~') {
					if (rlength == 1) {
						relPath = "";
					} else if (rlength > 1 && relPath [1] == '/') {
						relPath = relPath.Substring (2);
						slash = "/";
					}

					string appvpath = HttpRuntime.AppDomainAppVirtualPath;
					if (appvpath.EndsWith ("/"))
						slash = "";

					return Canonic (appvpath + slash + relPath);
				}

				return Canonic (basePath + slash + relPath);
			}

			if (basePath == null || basePath == "" || basePath [0] == '~')
				basePath = HttpRuntime.AppDomainAppVirtualPath;

			if (basePath.Length <= 1)
				basePath = String.Empty;

			return Canonic (basePath + "/" + relPath);
		}

		static char [] path_sep = {'\\', '/'};
		
		internal static string Canonic (string path)
		{
			string [] parts = path.Split (path_sep);
			int end = parts.Length;
			
			int dest = 0;
			
			for (int i = 0; i < end; i++) {
				string current = parts [i];
				if (current == "." )
					continue;

				if (current == "..") {
					if (dest == 0) {
						if (i == 1) // see bug 52599
							continue;

						throw new HttpException ("Invalid path.");
					}

					dest --;
					continue;
				}

				parts [dest++] = current;
			}

			if (dest == 0)
				return "/";

			return String.Join ("/", parts, 0, dest);
		}
		
		internal static string GetDirectory (string url)
		{
			url = url.Replace('\\','/');
			int last = url.LastIndexOf ('/');

			if (last > 0) {
#if NET_2_0
				return RemoveDoubleSlashes (url.Substring (0, last));
#else
				return url.Substring (0, last);
#endif
			}

			return "/";
		}

#if NET_2_0
		internal static string RemoveDoubleSlashes (string input)
		{
			// MS VirtualPathUtility removes duplicate '/'
			string str = input;
			string x;
			while ((x = str.Replace ("//", "/")) != str) {
				str = x;
			}

			return str;
		}
#endif

		internal static string GetFile (string url)
		{
			url = url.Replace('\\','/');
			int last = url.LastIndexOf ('/');
			if (last >= 0) {
				if (url.Length == 1) // Empty file name instead of ArgumentOutOfRange
					return "";
				return url.Substring (last+1);
			}

			throw new Exception (String.Format ("GetFile: `{0}' does not contain a /", url));
		}
		
		internal static bool IsRooted (string path)
		{
			if (path == null || path == "")
				return true;

			char c = path [0];
			if (c == '/' || c == '\\')
				return true;

			return false;
		}

		internal static bool IsRelativeUrl (string path)
		{
			return (path [0] != '/' && path.IndexOf (':') == -1);
		}

                public static string ResolveVirtualPathFromAppAbsolute (string path)
                {
                        if (path [0] != '~') return path;
                                
                        if (path.Length == 1)
                                return HttpRuntime.AppDomainAppVirtualPath;

                        if (path [1] == '/' || path [1] == '\\') {
                                string appPath = HttpRuntime.AppDomainAppVirtualPath;
                                if (appPath.Length > 1) 
                                        return appPath + "/" + path.Substring (2);
                                return "/" + path.Substring (2);
                        }
                        return path;    
                }

                public static string ResolvePhysicalPathFromAppAbsolute (string path) 
                {
                        if (path [0] != '~') return path;

                        if (path.Length == 1)
                                return HttpRuntime.AppDomainAppPath;

                        if (path [1] == '/' || path [1] == '\\') {
                                string appPath = HttpRuntime.AppDomainAppPath;
                                if (appPath.Length > 1)
                                        return appPath + "/" + path.Substring (2);
                                return "/" + path.Substring (2);
                        }
                        return path;
                }
	}
}
