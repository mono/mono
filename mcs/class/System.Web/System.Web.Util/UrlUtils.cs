
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
/**
 * Namespace: System.Web.UI.Util
 * Class:     UrlUtils
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Status:  ??%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Text;
using System.Web.SessionState;

namespace System.Web.Util
{
	internal class UrlUtils
	{
		/*
		 * I could not find these functions in the class System.Uri
		 * Besides, an instance of Uri will not be formed until and unless the address is of
		 * the form protocol://[user:pass]host[:port]/[fullpath]
		 * ie, a protocol, and that too without any blanks before,
		 * is a must which may not be the case here.
		 * Important: Escaped URL is assumed here. nothing like .aspx?path=/something
		 * It should be .aspx?path=%2Fsomething
		 */
		public static string GetProtocol(string url)
		{
			//Taking code from Java Class java.net.URL
			if(url!=null)
			{
				if(url.Length>0)
				{
					
					int i, start = 0, limit;
					limit = url.Length;
					char c;
					bool aRef = false;
					while( (limit > 0) && (url[limit-1] <= ' '))
					{
						limit --;
					}
					while( (start < limit) && (url[start] <= ' '))
					{
						start++;
					}
					if(RegionMatches(true, url, start, "url:", 0, 4))
					{
						start += 4;
					}
					if(start < url.Length && url[start]=='#')
					{
						aRef = true;
					}
					for(i = start; !aRef && (i < limit) && ((c=url[i]) != '/'); i++)
					{
						if(c==':')
						{
							return url.Substring(start, i - start);
						}
					}
				}
			}
			return String.Empty;
		}
		
		public static bool IsRelativeUrl(string url)
		{
			if (url.IndexOf(':') == -1)
				return !IsRooted(url);

			return false;
		}

		public static bool IsRootUrl(string url)
		{
			if(url!=null)
			{
				if(url.Length>0)
				{
					return IsValidProtocol(GetProtocol(url).ToLower());
				}
			}
			return true;
		}
		
		public static bool IsRooted(string path)
		{
			if(path!=null && path.Length > 0)
			{
				return (path[0]=='/' || path[0]=='\\');
			}
			return true;
		}
		
		public static void FailIfPhysicalPath(string path)
		{
			if(path!= null && path.Length > 1)
			{
				if(path[1]==':' || path.StartsWith(@"\\"))
					throw new HttpException(HttpRuntime.FormatResourceString("Physical_path_not_allowed", path));
			}
		}

		public static string Combine (string basePath, string relPath)
		{
			if (relPath == null)
				throw new ArgumentNullException ("relPath");

			int rlength = relPath.Length;
			if (rlength == 0)
				return "";

			FailIfPhysicalPath (relPath);
			relPath = relPath.Replace ("\\", "/");
			if (IsRooted (relPath))
				return Reduce (relPath);

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

					return Reduce (appvpath + slash + relPath);
				}

				return Reduce (basePath + slash + relPath);
			}

			if (basePath == null || basePath == "")
				basePath = HttpRuntime.AppDomainAppVirtualPath;

			if (basePath.Length <= 1)
				basePath = String.Empty;

			return Reduce (basePath + "/" + relPath);
		}

		public static bool IsValidProtocol(string protocol)
		{
			if(protocol.Length < 1)
				return false;
			char c = protocol[0];
			if(!Char.IsLetter(c))
			{
				return false;
			}
			for(int i=1; i < protocol.Length; i++)
			{
				c = protocol[i];
				if(!Char.IsLetterOrDigit(c) && c!='.' && c!='+' && c!='-')
				{
					return false;
				}
			}
			return true;
		}
		
		/*
		 * MakeRelative("http://www.foo.com/bar1/bar2/file","http://www.foo.com/bar1")
		 * will return "bar2/file"
		 * while MakeRelative("http://www.foo.com/bar1/...","http://www.anotherfoo.com")
		 * return 'null' and so does the call
		 * MakeRelative("http://www.foo.com/bar1/bar2","http://www.foo.com/bar")
		 */
		public static string MakeRelative(string fullUrl, string relativeTo)
		{
			if (fullUrl == relativeTo)
				return String.Empty;

			if (fullUrl.IndexOf (relativeTo) != 0)
				return null;

			string leftOver = fullUrl.Substring (relativeTo.Length);
			if (leftOver.Length > 0 && leftOver [0] == '/')
				leftOver = leftOver.Substring (1);

			leftOver = Reduce (leftOver);
			if (leftOver.Length > 0 && leftOver [0] == '/')
				leftOver = leftOver.Substring (1);

			return leftOver;
		}

		/*
		 * Check JavaDocs for java.lang.String#RegionMatches(bool, int, String, int, int)
		 * Could not find anything similar in the System.String class
		 */
		public static bool RegionMatches(bool ignoreCase, string source, int start, string match, int offset, int len)
		{
			if(source!=null || match!=null)
			{
				if(source.Length>0 && match.Length>0)
				{
					char[] ta = source.ToCharArray();
					char[] pa = match.ToCharArray();
					if((offset < 0) || (start < 0) || (start > (source.Length - len)) || (offset > (match.Length - len)))
					{
						return false;
					}
					while(len-- > 0)
					{
						char c1 = ta[start++];
						char c2 = pa[offset++];
						if(c1==c2)
							continue;
						if(ignoreCase)
						{
							if(Char.ToUpper(c1)==Char.ToUpper(c2))
								continue;
							// Check for Gregorian Calendar where the above may not hold good
							if(Char.ToLower(c1)==Char.ToLower(c2))
								continue;
						}
						return false;
					}
					return true;
				}
			}
			return false;
		}

		public static string Reduce (string path)
		{
			path = path.Replace ('\\','/');

			string [] parts = path.Split ('/');
			ArrayList result = new ArrayList ();
			
			int end = parts.Length;
			for (int i = 0; i < end; i++) {
				string current = parts [i];
				if (current == "." )
					continue;

				if (current == "..") {
					if (result.Count == 0) {
						if (i == 1) // see bug 52599
							continue;

						throw new HttpException ("Invalid path.");
					}

					result.RemoveAt (result.Count - 1);
					continue;
				}

				result.Add (current);
			}

			if (result.Count == 0)
				return "/";

			return String.Join ("/", (string []) result.ToArray (typeof (string)));
		}
		
		public static string GetDirectory(string url)
		{
			if(url==null)
			{
				return null;
			}
			if(url.Length==0)
			{
				return String.Empty;
			}
			url = url.Replace('\\','/');

			string baseDir = "";
			int last = url.LastIndexOf ('/');
			if (last > 0)
				baseDir = url.Substring(0, url.LastIndexOf('/'));

			if(baseDir.Length==0)
			{
				baseDir = "/";
			}
			return baseDir;
		}

		static string GetFile (string url)
		{
			if (url == null)
				return null;

			if (url.Length == 0)
				return String.Empty;

			url = url.Replace ('\\', '/');

			int last = url.LastIndexOf ('/') + 1;
			if (last != 0) {
				url = url.Substring (last);
			}

			return url;
		}

		// appRoot + SessionID + vpath
		public static string InsertSessionId (string id, string path)
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

			return Reduce (appvpath + "(" + id + ")/" + path);
		}

		public static string GetSessionId (string path)
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

		public static string RemoveSessionId (string base_path, string file_path)
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

			return Reduce (dir + GetFile (file_path));
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

