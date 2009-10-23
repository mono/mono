//
// UriTemplateMatch.cs
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;

#if NET_2_1
using NameValueCollection = System.Collections.Generic.Dictionary<string,string>;
#endif

namespace System
{
	public class UriTemplateMatch
	{
		public UriTemplateMatch ()
		{
		}

		Uri base_uri, request_uri;
		NameValueCollection nvc, query_params;
		object data;
		UriTemplate template;
		Collection<string> path_segments, wildcard;

		public Uri BaseUri {
			get { return base_uri; }
			set { base_uri = value; }
		}

		public NameValueCollection BoundVariables {
			get {
				if (nvc == null)
					nvc = new NameValueCollection ();
				return nvc;
			}
		}

		public object Data {
			get { return data; }
			set { data = value; }
		}

		public NameValueCollection QueryParameters {
			get {
				if (query_params == null)
					query_params = new NameValueCollection ();
				return query_params;
			}
		}

		public Collection<string> RelativePathSegments { 
			get {
				if (path_segments == null)
					path_segments = new Collection<string> ();
				return path_segments;
			}
		}

		public Uri RequestUri {
			get { return request_uri; }
			set { request_uri = value; }
		}

		public UriTemplate Template {
			get { return template; }
			set { template = value; }
		}

		public Collection<string> WildcardPathSegments {
			get {
				if (wildcard == null)
					wildcard = new Collection<string> ();
				return wildcard;
			}
		}
	}
}
