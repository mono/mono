//
// SystemWebTestShim/UrlUtils.cs
//
// Author:
//   Raja R Harinath (harinath@hurrynot.org)
//
// (C) 2009 Novell, Inc (http://www.novell.com)
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


//using Orig = System.Web.Util.UrlUtils;
using System;
using System.Text;
using System.Web;

namespace SystemWebTestShim {
	public class UrlUtils {
		public static string Canonic (string path)
		{
			return Canonic_Int (path);
		}

		static char [] path_sep = {'\\', '/'};

		public static string Canonic_Int (string path)
		{
			bool isRooted = IsRooted(path);
			bool endsWithSlash = path.EndsWith("/");
			string [] parts = path.Split (path_sep);
			int end = parts.Length;

			int dest = 0;

			for (int i = 0; i < end; i++) {
				string current = parts [i];

				if (current.Length == 0)
					continue;

				if (current == "." )
					continue;

				if (current == "..") {
					dest --;
					continue;
				}
				if (dest < 0)
					if (!isRooted)
						throw new HttpException ("Invalid path.");
					else
						dest = 0;

				parts [dest++] = current;
			}
			if (dest < 0)
				throw new HttpException ("Invalid path.");

			if (dest == 0)
				return "/";

			string str = String.Join ("/", parts, 0, dest);
			str = RemoveDoubleSlashes (str);
			if (isRooted)
				str = "/" + str;
			if (endsWithSlash)
				str = str + "/";

			return str;
		}

		public static string GetDirectory (string url)
		{
			url = url.Replace('\\','/');
			int last = url.LastIndexOf ('/');

			if (last > 0) {
				if (last < url.Length)
					last++;
				return RemoveDoubleSlashes (url.Substring (0, last));
			}

			return "/";
		}

		public static string RemoveDoubleSlashes (string input)
		{
			// MS VirtualPathUtility removes duplicate '/'

			int index = -1;
			for (int i = 1; i < input.Length; i++)
				if (input [i] == '/' && input [i - 1] == '/') {
					index = i - 1;
					break;
				}

			if (index == -1) // common case optimization
				return input;

			StringBuilder sb = new StringBuilder (input.Length);
			sb.Append (input, 0, index);

			for (int i = index; i < input.Length; i++) {
				if (input [i] == '/') {
					int next = i + 1;
					if (next < input.Length && input [next] == '/')
						continue;
					sb.Append ('/');
				}
				else {
					sb.Append (input [i]);
				}
			}

			return sb.ToString ();
		}

		public static string GetFile (string url)
		{
			url = url.Replace('\\','/');
			int last = url.LastIndexOf ('/');
			if (last >= 0) {
				if (url.Length == 1) // Empty file name instead of ArgumentOutOfRange
					return "";
				return url.Substring (last+1);
			}

			throw new ArgumentException (String.Format ("GetFile: `{0}' does not contain a /", url));
		}

		public static bool IsRooted (string path)
		{
			if (path == null || path.Length == 0)
				return true;

			char c = path [0];
			if (c == '/' || c == '\\')
				return true;

			return false;
		}

		public static bool IsRelativeUrl (string path)
		{
			return (path [0] != '/' && path.IndexOf (':') == -1);
		}


	}
}

