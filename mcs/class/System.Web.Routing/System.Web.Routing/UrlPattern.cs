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
		sealed class TrimEntry 
		{
			public bool Allowed;
		}
		
		int segmentsCount;
		string [] segments;
		int [] segmentLengths;
		bool [] segment_flags;
		List <string> tokens;

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

			tokens = new List<string> ();

			segments = Url.Split ('/');
			segmentsCount = segments.Length;
			segment_flags = new bool [segmentsCount];
			segmentLengths = new int [segmentsCount];
			
			for (int i = 0; i < segmentsCount; i++) {
				string s = segments [i];
				int slen = s.Length;
				segmentLengths [i] = slen;
				if (slen == 0 && i < segmentsCount - 1)
					throw new ArgumentException ("Consecutive URL segment separators '/' are not allowed");
				int from = 0;
				while (from < slen) {
					int start = s.IndexOf ('{', from);
					if (start == slen - 1)
						throw new ArgumentException ("Unterminated URL parameter. It must contain matching '}'");
					if (start < 0) {
						if (s.IndexOf ('}', from) >= from)
							throw new ArgumentException ("Unmatched URL parameter closer '}'. A corresponding '{' must precede");
						from = slen;
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
		}

		string SegmentToKey (string segment)
		{
			int start = segment.IndexOf ('{');
			int end = segment.IndexOf ('}');
			if (start == -1)
				start = 0;
			else
				start++;
			
			if (end == -1)
				end = segment.Length;
			return segment.Substring (start, end - start);
		}

		RouteValueDictionary AddDefaults (RouteValueDictionary dict, RouteValueDictionary defaults)
		{
			if (defaults != null && defaults.Count > 0) {
				string key;
				foreach (var def in defaults) {
					key = def.Key;
					if (dict.ContainsKey (key))
						continue;
					dict.Add (key, def.Value);
				}
			}

			return dict;
		}
		
		RouteValueDictionary tmp = new RouteValueDictionary ();
		public RouteValueDictionary Match (string path, RouteValueDictionary defaults)
		{
			tmp.Clear ();

			// quick check
			if (Url == path && Url.IndexOf ('{') < 0)
				return AddDefaults (tmp, defaults);

			string [] argSegs = path.Split ('/');
			int argsLen = argSegs.Length;
			bool haveDefaults = defaults != null && defaults.Count > 0;
			if (!haveDefaults && argsLen != segmentsCount)
				return null;

			for (int i = 0; i < segmentsCount; i++) {
				if (segment_flags [i]) {
					string t = segments [i];
					string v = i < argsLen ? argSegs [i] : null;

					if (String.IsNullOrEmpty (v)) {
						string key = SegmentToKey (segments [i]);
						object o;
						if (haveDefaults && !defaults.TryGetValue (key, out o))
							return null; // ends with '/' while more
								     // tokens are expected and
								     // there are is no default
								     // value in the defaults
								     // dictionary.

						v = o as string;
						if (v == null)
							throw new InvalidOperationException ("The RouteData must contain an item named '" + key + "' with a string value.");
					}
					
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
				} else if (i > argsLen || segments [i] != argSegs [i])
					return null;
			}

			return AddDefaults (tmp, defaults);
		}

		static readonly string [] substsep = {"{{"};
		
		// it may return null for invalid values.
		public bool TrySubstitute (RouteValueDictionary values, RouteValueDictionary defaults, out string value)
		{
			if (values == null) {
				value = Url;
				return true;
			}

			var trim = new Dictionary <string, TrimEntry> (StringComparer.OrdinalIgnoreCase);
			foreach (string token in tokens) {
				bool trimValue;
				if (!values.ContainsKey (token)) {
					value = null;
					return false;
				}

				object left, right;
				if (values.TryGetValue (token, out left) && defaults != null && defaults.TryGetValue (token, out right)) {
					if (left is string && right is string)
						trimValue = String.Compare ((string)left, (string)right, StringComparison.OrdinalIgnoreCase) == 0;
					else if (left == right)
						trimValue = true;
					else
						trimValue = false;
				} else
					trimValue = false;

				if (!trimValue) {
					foreach (var de in trim)
						de.Value.Allowed = false;
				}
				
				if (!trim.ContainsKey (token))
					trim.Add (token, new TrimEntry { Allowed = trimValue});
			}

			var replacements = new RouteValueDictionary (values);
			if (defaults != null) {
				string key;
				
				foreach (var de in defaults) {
					key = de.Key;
					if (replacements.ContainsKey (key))
						continue;
					replacements.Add (key, de.Value);
				}
			}
			
			// horrible hack, but should work
			string [] arr = Url.Split (substsep, StringSplitOptions.None);
			for (int i = 0; i < arr.Length; i++) {
				string s = arr [i];
				foreach (var p in replacements) {
					string pKey = p.Key;
					string pValue;
					TrimEntry trimValue;
					
					if (trim.TryGetValue (pKey, out trimValue)) {
						if (trimValue.Allowed)
							pValue = String.Empty;
						else
							pValue = p.Value.ToString ();
					} else
						pValue = p.Value.ToString ();

					s = s.Replace ("{" + pKey + "}", pValue);
				}
				
				arr [i] = s;
			}
			value = String.Join ("{{", arr);
			return true;
		}
	}
}
