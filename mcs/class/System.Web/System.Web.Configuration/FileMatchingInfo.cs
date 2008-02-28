//
// System.Web.Configuration.HttpHandlerAction
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Web;
using System.Text.RegularExpressions;

namespace System.Web.Configuration 
{
	internal class FileMatchingInfo
	{
		public string MatchExact;
		public string MatchExpr;

		// If set, we can fast-path the patch with string.EndsWith (FMI.EndsWith)
		public string EndsWith;
		public Regex RegExp;
		
		public FileMatchingInfo (string s)
		{
			MatchExpr = s;
			int len = s.Length;

			if (len > 0) {
				if (s[0] == '*' && (s.IndexOf ('*', 1) == -1))
					EndsWith = s.Substring (1);

				if (s.IndexOf ('*') == -1)
					if (s [0] != '/') {
						HttpContext ctx = HttpContext.Current;
						HttpRequest req = ctx != null ? ctx.Request : null;
						string vpath =  req != null ? req.BaseVirtualDir : HttpRuntime.AppDomainAppVirtualPath;
						
						if (vpath == "/")
							vpath = String.Empty;
						
						MatchExact = String.Concat (vpath, "/", s);
					}
			}
				
			if (MatchExpr != "*") {
				string expr = MatchExpr.Replace(".", "\\.").Replace("?", "\\?").Replace("*", ".*");
				if (expr.Length > 0 && expr [0] =='/')
					expr = expr.Substring (1);

				expr += "\\z";
				RegExp = new Regex (expr, RegexOptions.IgnoreCase);
			}
		}
	}
}
