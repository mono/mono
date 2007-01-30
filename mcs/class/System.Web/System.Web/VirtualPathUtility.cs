//
// System.Web.VirtualPathUtility.cs
//
// Author:
//	Chris Toshok (toshok@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
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

#if NET_2_0

using System.Web.Util;

namespace System.Web {

	public static class VirtualPathUtility
	{
		public static string AppendTrailingSlash (string virtualPath)
		{
			if (virtualPath == null)
				return virtualPath;

			int length = virtualPath.Length;
			if (length == 0 || virtualPath [length - 1] == '/')
				return virtualPath;

			return virtualPath + "/";
		}

		public static string Combine (string basePath, string relativePath)
		{
			if (basePath == null || basePath == "")
				throw new ArgumentNullException ("basePath");

			if (relativePath == null)
				// LAME SPEC - MSDN Specifies ArgumentNullException but MS throws ArgumentException
				throw new ArgumentException ("relativePath");

			if (relativePath == "")
				// MS throw different exception for null or empty string.
				throw new ArgumentNullException ("relativePath");

			if (relativePath.Length == 1 && relativePath [0] == '~')
				return HttpRuntime.AppDomainAppVirtualPath;

			if (relativePath [0] == '~' && relativePath [1] == '/')
				return UrlUtils.RemoveDoubleSlashes ((HttpRuntime.AppDomainAppVirtualPath + relativePath.Substring (1)).Replace ('\\', '/'));

			if (basePath [0] != '/')
				throw new ArgumentException ("basePath is not an absolute path", "basePath");

			if (basePath.Length > 1 && basePath [basePath.Length - 1] != '/') {
				int lastSlash = basePath.LastIndexOf ('/');
				if (lastSlash >= 0)
					return UrlUtils.Combine (basePath.Substring (0, lastSlash + 1), relativePath);
			}

			return UrlUtils.Combine (basePath, relativePath);
		}

		public static string GetDirectory (string virtualPath)
		{
			if (virtualPath == null || virtualPath == "") // Yes, "" throws an ArgumentNullException
				throw new ArgumentNullException ("virtualPath");

			if (virtualPath [0] != '/')
				throw new ArgumentException ("The virtual path is not rooted", "virtualPath");

            		if (virtualPath == "/")
                		return null; //.net behavior

            		//In .Net - will look for one '/' before the last one, and will return it as a directory
            		//therefor we always should remove the last slash.
            		if (virtualPath.EndsWith("/")) 
                		virtualPath = virtualPath.Substring(0, virtualPath.Length - 1);

            		string result = UrlUtils.GetDirectory (virtualPath);
			return AppendTrailingSlash (result);
		}

		public static string GetExtension (string virtualPath)
		{
            		if (virtualPath != null && virtualPath != "" && 
				virtualPath.IndexOf('/') == -1)
                	{
				virtualPath = "./" + virtualPath;
			}

            		string filename = GetFileName (virtualPath);
			int dot = filename.LastIndexOf ('.');
			if (dot == -1 || dot == filename.Length + 1)
				return "";

			return filename.Substring (dot);
		}

		public static string GetFileName (string virtualPath)
		{
			if (virtualPath == null || virtualPath == "") // Yes, "" throws an ArgumentNullException
				throw new ArgumentNullException ("virtualPath");
			
			return UrlUtils.GetFile (RemoveTrailingSlash (virtualPath));
		}

		public static bool IsAbsolute (string virtualPath)
		{
			if (virtualPath == "" || virtualPath == null)
				throw new ArgumentNullException ("virtualPath");

			return (virtualPath [0] == '/');
		}

		public static bool IsAppRelative (string virtualPath)
		{
			if (String.IsNullOrEmpty (virtualPath))
				throw new ArgumentNullException ("virtualPath");

			if (virtualPath.Length == 1 && virtualPath [0] == '~')
				return true;

			if (virtualPath [0] == '~' && virtualPath [1] == '/')
				return true;

			return false;
		}

		public static string MakeRelative (string fromPath, string toPath)
		{
			if (fromPath == null || toPath == null)
				throw new NullReferenceException (); // yeah!

			if (toPath == "")
				return toPath;

			if (toPath [0] != '/')
				throw new ArgumentOutOfRangeException (); // This is what MS does.

			if (fromPath.Length > 0 && fromPath [0] != '/')
				throw new ArgumentOutOfRangeException (); // This is what MS does.

			Uri from = new Uri ("http://nothing" + fromPath);
			return from.MakeRelativeUri (new Uri ("http://nothing" + toPath)).AbsolutePath;
		}

		public static string RemoveTrailingSlash (string virtualPath)
		{
			if (virtualPath == null || virtualPath == "")
				return null;

			int last = virtualPath.Length - 1;
			if (last == 0 || virtualPath [last] != '/')
				return virtualPath;

			return virtualPath.Substring (0, last);
		}

		public static string ToAbsolute (string virtualPath)
		{
			string apppath = HttpRuntime.AppDomainAppVirtualPath;
			if (apppath == null)
				throw new HttpException ("The path to the application is not known");

			return ToAbsolute (virtualPath,apppath);
		}

		// If virtualPath is: 
		// Absolute, the ToAbsolute method returns the virtual path with no changes.
		// Application relative, the ToAbsolute method adds applicationPath to the beginning of the virtual path.
		// Not rooted, the ToAbsolute method raises an ArgumentOutOfRangeException exception.
		public static string ToAbsolute (string virtualPath, string applicationPath)
		{
			if (applicationPath == null || applicationPath == "")
				throw new ArgumentNullException ("applicationPath");

			if (virtualPath == null || virtualPath == "")
				throw new ArgumentNullException ("virtualPath");

			if (IsAppRelative(virtualPath)) {
				if (applicationPath [0] != '/')
					throw new ArgumentException ("appPath is not rooted", "applicationPath");
				return UrlUtils.RemoveDoubleSlashes ((applicationPath + (virtualPath.Length == 1 ? "/" : virtualPath.Substring (1))).Replace ('\\', '/'));
			}

			if (virtualPath [0] != '/')
				throw new ArgumentException (String.Format ("Relative path not allowed: '{0}'", virtualPath));

			return UrlUtils.RemoveDoubleSlashes (virtualPath.Replace ('\\', '/'));

		}

		public static string ToAppRelative (string virtualPath)
		{
			string apppath = HttpRuntime.AppDomainAppVirtualPath;
			if (apppath == null)
				throw new HttpException ("The path to the application is not known");

			return ToAppRelative (apppath, virtualPath);
		}

		public static string ToAppRelative (string virtualPath, string applicationPath)
		{
			if (applicationPath == null || applicationPath == "")
				throw new ArgumentNullException ("applicationPath");

			if (virtualPath == null || virtualPath == "")
				throw new ArgumentNullException ("virtualPath");

			if (virtualPath.StartsWith (".."))
				throw new ArgumentException (String.Format ("Relative path not allowed: '{0}'", virtualPath));

			if (applicationPath [0] != '/')
				throw new ArgumentOutOfRangeException ("appPath is not rooted", "applicationPath");

			return MakeRelative (applicationPath, virtualPath);
		}
	}
}

#endif



