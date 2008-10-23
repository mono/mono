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
using System.Collections.Generic;
using System.Security.Permissions;
using System.Web;

namespace System.Web.Routing
{
	internal class UrlPattern
	{
		string [] segments;
		bool [] segment_flags;
		string [] tokens;

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

			var tokens = new List<string> ();

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
					string token = s.Substring (start + 1, end - start - 1);
					if (!tokens.Contains (token))
						tokens.Add (token);
					from = end + 1;
				}
			}

			this.tokens = tokens.ToArray ();
		}

		RouteValueDictionary tmp = new RouteValueDictionary ();

		// FIXME: how is "defaults" used?
		public RouteValueDictionary Match (string path, RouteValueDictionary defaults)
		{
			tmp.Clear ();

			// quick check
			if (Url == path && Url.IndexOf ('{') < 0)
				return tmp;

			string [] argSegs = path.Split ('/');
			if (argSegs.Length != segments.Length)
				return null;

			for (int i = 0; i < segments.Length; i++) {
				if (segment_flags [i]) {
					string t = segments [i];
					string v = argSegs [i];
					if (v.Length == 0)
						return null; // ends with '/' while more tokens are expected.
					int tfrom = 0, vfrom = 0;
					while (tfrom < t.Length) {
						int start = t.IndexOf ('{', tfrom);
						if (start < 0) {
							int tlen = t.Length - tfrom;
							int vlen = v.Length - vfrom;
							if (tlen != vlen ||
							    String.Compare (t, tfrom, v, vfrom, tlen, StringComparison.Ordinal) != 0)
								return null; // mismatch
							break;
						}

						// if there is a string literal before next template item, check it in the value string.
						int len = start - tfrom;
						if (len > 0 && String.CompareOrdinal (t, tfrom, v, vfrom, len) != 0)
							return null; // mismatch
						vfrom += len;

						int end = t.IndexOf ('}', start + 1);
						int next = t.IndexOf ('{', end + 1);
						string key = t.Substring (start + 1, end - start - 1);
						string nextToken = next < 0 ? t.Substring (end + 1) : t.Substring (end + 1, next - end - 1);
						int vnext = nextToken.Length > 0 ? v.IndexOf (nextToken, vfrom + 1, StringComparison.Ordinal) : -1;

						if (next < 0) {
							var vs = vnext < 0 ? v.Substring (vfrom) : v.Substring (vfrom, vnext - vfrom);
							tmp.Add (key, vs);
							vfrom = vs.Length;
						} else {
							if (vnext < 0)
								return null; // mismatch
							tmp.Add (key, v.Substring (vfrom, vnext - vfrom));
							vfrom = vnext;
						}
						tfrom = end + 1;
					}
				} else if (segments [i] != argSegs [i])
					return null;
			}

			return tmp;
		}

		static readonly string [] substsep = {"{{"};

		// it may return null for invalid values.
		public bool TrySubstitute (RouteValueDictionary values, out string value)
		{
			if (values == null) {
				value = Url;
				return true;
			} else {
				foreach (string token in tokens) {
					if (!values.ContainsKey (token)) {
						value = null;
						return false;
					}
				}
			}

			// horrible hack, but should work
			string [] arr = Url.Split (substsep, StringSplitOptions.None);
			for (int i = 0; i < arr.Length; i++) {
				string s = arr [i];
				foreach (var p in values)
					s = s.Replace ("{" + p.Key + "}", p.Value.ToString ());
				arr [i] = s;
			}
			value = String.Join ("{{", arr);
			return true;
		}
	}
}
