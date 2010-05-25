//
// UriTemplate.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;

#if NET_2_1
using NameValueCollection = System.Object;
#endif

namespace System
{
	public class UriTemplate
	{
		static readonly ReadOnlyCollection<string> empty_strings = new ReadOnlyCollection<string> (new string [0]);

		string template;
		ReadOnlyCollection<string> path, query;
		Dictionary<string,string> query_params = new Dictionary<string,string> ();

		public UriTemplate (string template)
			: this (template, false)
		{
		}

		public UriTemplate (string template, IDictionary<string,string> additionalDefaults)
			: this (template, false, additionalDefaults)
		{
		}

		public UriTemplate (string template, bool ignoreTrailingSlash)
			: this (template, ignoreTrailingSlash, null)
		{
		}

		public UriTemplate (string template, bool ignoreTrailingSlash, IDictionary<string,string> additionalDefaults)
		{
			if (template == null)
				throw new ArgumentNullException ("template");
			this.template = template;
			IgnoreTrailingSlash = ignoreTrailingSlash;
			Defaults = new Dictionary<string,string> (StringComparer.InvariantCultureIgnoreCase);
			if (additionalDefaults != null)
				foreach (var pair in additionalDefaults)
					Defaults.Add (pair.Key, pair.Value);

			string p = template;
			// Trim scheme, host name and port if exist.
			if (CultureInfo.InvariantCulture.CompareInfo.IsPrefix (template, "http")) {
				int idx = template.IndexOf ('/', 8); // after "http://x" or "https://"
				if (idx > 0)
					p = template.Substring (idx);
			}
			int q = p.IndexOf ('?');
			path = ParsePathTemplate (p, 0, q >= 0 ? q : p.Length);
			if (q >= 0)
				ParseQueryTemplate (p, q, p.Length);
			else
				query = empty_strings;
		}

		public bool IgnoreTrailingSlash { get; private set; }

		public IDictionary<string,string> Defaults { get; private set; }

		public ReadOnlyCollection<string> PathSegmentVariableNames {
			get { return path; }
		}

		public ReadOnlyCollection<string> QueryValueVariableNames {
			get { return query; }
		}

		public override string ToString ()
		{
			return template;
		}

		// Bind

#if !NET_2_1
		public Uri BindByName (Uri baseAddress, NameValueCollection parameters)
		{
			return BindByName (baseAddress, parameters, false);
		}

		public Uri BindByName (Uri baseAddress, NameValueCollection parameters, bool omitDefaults)
		{
			return BindByNameCommon (baseAddress, parameters, null, omitDefaults);
		}
#endif

		public Uri BindByName (Uri baseAddress, IDictionary<string,string> parameters)
		{
			return BindByName (baseAddress, parameters, false);
		}

		public Uri BindByName (Uri baseAddress, IDictionary<string,string> parameters, bool omitDefaults)
		{
			return BindByNameCommon (baseAddress, null, parameters, omitDefaults);
		}

		Uri BindByNameCommon (Uri baseAddress, NameValueCollection nvc, IDictionary<string,string> dic, bool omitDefaults)
		{
			CheckBaseAddress (baseAddress);

			// take care of case sensitivity.
			if (dic != null)
				dic = new Dictionary<string,string> (dic, StringComparer.OrdinalIgnoreCase);

			int src = 0;
			StringBuilder sb = new StringBuilder (template.Length);
			BindByName (ref src, sb, path, nvc, dic, omitDefaults);
			BindByName (ref src, sb, query, nvc, dic, omitDefaults);
			sb.Append (template.Substring (src));
			return new Uri (baseAddress.ToString () + sb.ToString ());
		}

		void BindByName (ref int src, StringBuilder sb, ReadOnlyCollection<string> names, NameValueCollection nvc, IDictionary<string,string> dic, bool omitDefaults)
		{
			foreach (string name in names) {
				int s = template.IndexOf ('{', src);
				int e = template.IndexOf ('}', s + 1);
				sb.Append (template.Substring (src, s - src));
#if NET_2_1
				string value = null;
#else
				string value = nvc != null ? nvc [name] : null;
#endif
				if (dic != null)
					dic.TryGetValue (name, out value);
				if (value == null && (omitDefaults || !Defaults.TryGetValue (name, out value)))
					throw new ArgumentException (String.Format ("The argument name value collection does not contain non-null value for '{0}'", name), "parameters");
				sb.Append (value);
				src = e + 1;
			}
		}

		public Uri BindByPosition (Uri baseAddress, params string [] values)
		{
			CheckBaseAddress (baseAddress);

			if (values.Length != path.Count + query.Count)
				throw new FormatException (String.Format ("Template '{0}' contains {1} parameters but the argument values to bind are {2}", template, path.Count + query.Count, values.Length));

			int src = 0, index = 0;
			StringBuilder sb = new StringBuilder (template.Length);
			BindByPosition (ref src, sb, path, values, ref index);
			BindByPosition (ref src, sb, query, values, ref index);
			sb.Append (template.Substring (src));
			return new Uri (baseAddress.ToString () + sb.ToString ());
		}

		void BindByPosition (ref int src, StringBuilder sb, ReadOnlyCollection<string> names, string [] values, ref int index)
		{
			for (int i = 0; i < names.Count; i++) {
				int s = template.IndexOf ('{', src);
				int e = template.IndexOf ('}', s + 1);
				sb.Append (template.Substring (src, s - src));
				string value = values [index++];
				if (value == null)
					throw new FormatException (String.Format ("The argument value collection contains null at {0}", index - 1));
				sb.Append (value);
				src = e + 1;
			}
		}

		// Compare

		public bool IsEquivalentTo (UriTemplate other)
		{
			if (other == null)
				throw new ArgumentNullException ("other");
			return this.template == other.template;
		}

		// Match

		static readonly char [] slashSep = {'/'};

		public UriTemplateMatch Match (Uri baseAddress, Uri candidate)
		{
			CheckBaseAddress (baseAddress);
			if (candidate == null)
				throw new ArgumentNullException ("candidate");

			var us = baseAddress.LocalPath;
			if (us [us.Length - 1] != '/')
				baseAddress = new Uri (baseAddress.GetComponents (UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped) + '/' + baseAddress.Query, baseAddress.IsAbsoluteUri ? UriKind.Absolute : UriKind.RelativeOrAbsolute);
			if (IgnoreTrailingSlash) {
				us = candidate.LocalPath;
				if (us.Length > 0 && us [us.Length - 1] != '/')
					candidate = new Uri(candidate.GetComponents (UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped) + '/' + candidate.Query, candidate.IsAbsoluteUri ? UriKind.Absolute : UriKind.RelativeOrAbsolute);
			}

			if (Uri.Compare (baseAddress, candidate, UriComponents.StrongAuthority, UriFormat.SafeUnescaped, StringComparison.Ordinal) != 0)
				return null;

			int i = 0, c = 0;
			UriTemplateMatch m = new UriTemplateMatch ();
			m.BaseUri = baseAddress;
			m.Template = this;
			m.RequestUri = candidate;
			var vc = m.BoundVariables;

			string cp = Uri.UnescapeDataString (baseAddress.MakeRelativeUri (candidate).ToString ());
			if (IgnoreTrailingSlash && cp [cp.Length - 1] == '/')
				cp = cp.Substring (0, cp.Length - 1);

			int tEndCp = cp.IndexOf ('?');
			if (tEndCp >= 0)
				cp = cp.Substring (0, tEndCp);

			if (template.Length > 0 && template [0] == '/')
				i++;
			if (cp.Length > 0 && cp [0] == '/')
				c++;

			foreach (string name in path) {
				int n = StringIndexOf (template, '{' + name + '}', i);
				if (String.CompareOrdinal (cp, c, template, i, n - i) != 0)
					return null; // doesn't match before current template part.
				c += n - i;
				i = n + 2 + name.Length;
				int ce = cp.IndexOf ('/', c);
				if (ce < 0)
					ce = cp.Length;
				string value = cp.Substring (c, ce - c);
				if (value.Length == 0)
					return null; // empty => mismatch
				vc [name] = value;
				m.RelativePathSegments.Add (value);
				c += value.Length;
			}
			int tEnd = template.IndexOf ('?');
			if (tEnd < 0)
				tEnd = template.Length;
			bool wild = (template [tEnd - 1] == '*');
			if (wild)
				tEnd--;
			if (!wild && (cp.Length - c) != (tEnd - i) ||
			    String.CompareOrdinal (cp, c, template, i, tEnd - i) != 0)
				return null; // suffix doesn't match
			if (wild) {
				c += tEnd - i;
				foreach (var pe in cp.Substring (c).Split (slashSep, StringSplitOptions.RemoveEmptyEntries))
					m.WildcardPathSegments.Add (pe);
			}
			if (candidate.Query.Length == 0)
				return m;


			string [] parameters = Uri.UnescapeDataString (candidate.Query.Substring (1)).Split ('&'); // chop first '?'
			foreach (string parameter in parameters) {
				string [] pair = parameter.Split ('=');
				m.QueryParameters.Add (pair [0], pair [1]);
				if (!query_params.ContainsKey (pair [0]))
					continue;
				string templateName = query_params [pair [0]];
				vc.Add (templateName, pair [1]);
			}

			return m;
		}

		int StringIndexOf (string s, string pattern, int idx)
		{
			return CultureInfo.InvariantCulture.CompareInfo.IndexOf (s, pattern, idx, CompareOptions.OrdinalIgnoreCase);
		}

		// Helpers

		void CheckBaseAddress (Uri baseAddress)
		{
			if (baseAddress == null)
				throw new ArgumentNullException ("baseAddress");
			if (!baseAddress.IsAbsoluteUri)
				throw new ArgumentException ("baseAddress must be an absolute URI.");
			if (baseAddress.Scheme == Uri.UriSchemeHttp ||
			    baseAddress.Scheme == Uri.UriSchemeHttps)
				return;
			throw new ArgumentException ("baseAddress scheme must be either http or https.");
		}

		ReadOnlyCollection<string> ParsePathTemplate (string template, int index, int end)
		{
			int widx = template.IndexOf ('*', index, end);
			if (widx >= 0 && widx != end - 1)
				throw new FormatException (String.Format ("Wildcard in UriTemplate is valid only if it is placed at the last part of the path: '{0}'", template));
			List<string> list = null;
			int prevEnd = -2;
			for (int i = index; i <= end; ) {
				i = template.IndexOf ('{', i);
				if (i < 0 || i > end)
					break;
				if (i == prevEnd + 1)
					throw new ArgumentException (String.Format ("The UriTemplate '{0}' contains adjacent templated segments, which is invalid.", template));
				int e = template.IndexOf ('}', i + 1);
				if (e < 0 || i > end)
					throw new FormatException (String.Format ("Missing '}' in URI template '{0}'", template));
				prevEnd = e;
				if (list == null)
					list = new List<string> ();
				i++;
				string name = template.Substring (i, e - i);
				string uname = name.ToUpper (CultureInfo.InvariantCulture);
				if (list.Contains (uname) || (path != null && path.Contains (uname)))
					throw new InvalidOperationException (String.Format ("The URI template string contains duplicate template item {{'{0}'}}", name));
				list.Add (uname);
				i = e + 1;
			}
			return list != null ? new ReadOnlyCollection<string> (list) : empty_strings;
		}

		void ParseQueryTemplate (string template, int index, int end)
		{
			// template starts with '?'
			string [] parameters = template.Substring (index + 1, end - index - 1).Split ('&');
			List<string> list = null;
			foreach (string parameter in parameters) {
				string [] pair = parameter.Split ('=');
				if (pair.Length != 2)
					throw new FormatException ("Invalid URI query string format");
				string pname = pair [0];
				string pvalue = pair [1];
				if (pvalue.Length >= 2 && pvalue [0] == '{' && pvalue [pvalue.Length - 1] == '}') {
					string ptemplate = pvalue.Substring (1, pvalue.Length - 2).ToUpper (CultureInfo.InvariantCulture);
					query_params.Add (pname, ptemplate);
					if (list == null)
						list = new List<string> ();
					if (list.Contains (ptemplate) || (path != null && path.Contains (ptemplate)))
						throw new InvalidOperationException (String.Format ("The URI template string contains duplicate template item {{'{0}'}}", pvalue));
					list.Add (ptemplate);
				}
			}
			query = list != null ? new ReadOnlyCollection<string> (list.ToArray ()) : empty_strings;
		}
	}
}
