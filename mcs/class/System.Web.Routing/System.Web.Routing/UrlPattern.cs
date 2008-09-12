//
// Route.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
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
using System.Security.Permissions;
using System.Web;

namespace System.Web.Routing
{
	internal class UrlPattern
	{
		string [] segments;
		bool [] segment_flags;

		public UrlPattern (string url)
		{
			Url = url;
			Parse ();
		}

		public string Url { get; private set; }

		void Parse ()
		{
			if (String.IsNullOrEmpty (Url))
				throw new SystemException ("INTERNAL ERROR: it should not try to parse null or empty string");
			if (Url [0] == '~' || Url [0] == '/')
				throw new ArgumentException ("Url must not start with '~' or '/'");
			if (Url.IndexOf ('?') >= 0)
				throw new ArgumentException ("Url must not contain '?'");
			segments = Url.Split ('/');
			segment_flags = new bool [segments.Length];
			for (int i = 0; i < segments.Length; i++) {
				string s = segments [i];
				if (s.Length == 0 && i < segments.Length - 1)
					throw new ArgumentException ("Consecutive URL segment separators '/' are not allowed");
				int from = 0;
				while (from < s.Length) {
					int start = s.IndexOf ('{', from);
					if (start == s.Length - 1)
						throw new ArgumentException ("Unterminated URL parameter. It must contain matching '}'");
					if (start < 0) {
						if (s.IndexOf ('}', from) >= from)
							throw new ArgumentException ("Unmatched URL parameter closer '}'. A corresponding '{' must precede");
						from = s.Length;
						continue;
					}
					segment_flags [i] = true;
					int end = s.IndexOf ('}', start + 1);
					int next = s.IndexOf ('{', start + 1);
					if (end < 0 || next >= 0 && next < end)
						throw new ArgumentException ("Unterminated URL parameter. It must contain matching '}'");
					if (end == start + 1)
						throw new ArgumentException ("Empty URL parameter name is not allowed");
					if (next == end + 1)
						throw new ArgumentException ("Two consecutive URL parameters are not allowed. Split into a different segment by '/', or a literal string.");
					from = end + 1;
				}
			}
		}
	}
}
