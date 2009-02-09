//
// System.UriData class
//
// Author:
//	Raja R Harinath <harinath@hurrynot.org>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Globalization;
using System.Security.Permissions;
using System.Text;

namespace System {
	class UriData : IUriData {
		Uri uri;
		UriParser parser;

		public UriData (Uri uri, UriParser parser)
		{
			this.uri = uri;
			this.parser = parser;
		}

		string Lookup (ref string cache, UriComponents components)
		{
			return Lookup (ref cache, components, uri.UserEscaped ? UriFormat.Unescaped : UriFormat.UriEscaped);
		}

		string Lookup (ref string cache, UriComponents components, UriFormat format)
		{
			if (cache == null)
				cache = parser.GetComponents (uri, components, format);
			return cache;
		}

		string absolute_path;
		public string AbsolutePath {
			get { return Lookup ( ref absolute_path, UriComponents.Path | UriComponents.KeepDelimiter);
			}
		}

		string absolute_uri;
		public string AbsoluteUri {
			get { return Lookup (ref absolute_uri, UriComponents.AbsoluteUri); }
		}

		string absolute_uri_unescaped;
		public string AbsoluteUri_SafeUnescaped {
			get { return Lookup (ref absolute_uri_unescaped, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped); }
		}

		string authority;
		public string Authority {
			get { return Lookup (ref authority, UriComponents.Host | UriComponents.Port); }
		}

		string fragment;
		public string Fragment {
			get { return Lookup (ref fragment, UriComponents.Fragment | UriComponents.KeepDelimiter); }
		}

		string host;
		public string Host {
			get { return Lookup (ref host, UriComponents.Host); }
		}

		string path_and_query;
		public string PathAndQuery {
			get { return Lookup (ref path_and_query, UriComponents.PathAndQuery); }
		}

		string strong_port;
		public string StrongPort {
			get { return Lookup (ref strong_port, UriComponents.StrongPort); }
		}

		string query;
		public string Query {
			get { return Lookup (ref query, UriComponents.Query | UriComponents.KeepDelimiter); }
		}

		string user_info;
		public string UserInfo {
			get { return Lookup (ref user_info, UriComponents.UserInfo); }
		}
	}
}
